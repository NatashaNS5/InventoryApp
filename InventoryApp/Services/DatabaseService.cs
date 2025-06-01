using InventoryApp.Data;
using InventoryApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace InventoryApp.Services
{
    public class DatabaseService
    {
        private readonly ApplicationDbContext _context;

        public DatabaseService()
        {
            _context = new ApplicationDbContext();
        }

        public List<Material> GetMaterials()
        {
            return _context.Materials
                .Include(m => m.InventoryRecords)
                .ToList();
        }

        public List<Product> GetProducts()
        {
            return _context.Products.ToList();
        }

        public List<Order> GetOrders()
        {
            try
            {
                var orders = _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .ToList();

                foreach (var order in orders)
                {
                    Debug.WriteLine($"Заказ {order.OrderNumber}: Количество элементов = {order.OrderItems?.Count ?? 0}");
                    order.TotalItems = order.OrderItems?.Sum(oi => oi.Quantity) ?? 0;
                }
                return orders;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка в GetOrders: {ex.Message}");
                throw;
            }
        }

        public List<OrderItem> GetOrderItems(int orderId)
        {
            return _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Include(oi => oi.Product)
                .Select(oi => new OrderItem
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    ProductName = oi.Product.Name
                })
                .ToList();
        }

        public void SaveInventory(List<Material> materials, bool isApproved)
        {
            foreach (var material in materials)
            {
                var inventoryRecord = new InventoryRecord
                {
                    MaterialId = material.Id,
                    ActualQuantity = material.ActualQuantity,
                    InventoryDate = DateTime.Now,
                    IsApproved = isApproved
                };
                _context.InventoryRecords.Add(inventoryRecord);

                var dbMaterial = _context.Materials.Find(material.Id);
                if (dbMaterial != null)
                {
                    dbMaterial.StockQuantity = material.ActualQuantity;
                    dbMaterial.ActualQuantity = material.ActualQuantity;
                    dbMaterial.DiscrepancyPercentage = material.StockQuantity != 0
                        ? (material.ActualQuantity - material.StockQuantity) / material.StockQuantity * 100
                        : 0;
                }
            }
            _context.SaveChanges();
        }

        public void SaveOrder(Order order, List<OrderItem> items)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                _context.Orders.Add(order);
                _context.SaveChanges();

                foreach (var item in items)
                {
                    item.OrderId = order.Id;
                    _context.OrderItems.Add(item);
                }
                _context.SaveChanges();
                transaction.Commit();
            }
        }

        public void SaveProduction(ProductionRecord record)
        {
            try
            {
                if (record == null)
                    throw new ArgumentNullException(nameof(record), "Запись производства не может быть null.");
                if (!_context.Products.Any(p => p.Id == record.ProductId))
                    throw new ArgumentException($"Изделие с ID {record.ProductId} не найдено.");
                if (!_context.Materials.Any(m => m.Id == record.MaterialId))
                    throw new ArgumentException($"Материал с ID {record.MaterialId} не найдено.");

                var material = _context.Materials.Find(record.MaterialId);
                if (material != null)
                {
                    if (material.StockQuantity < record.ActualMaterialUsage)
                        throw new InvalidOperationException($"Недостаточно материала {material.Name}. Остаток: {material.StockQuantity}, требуется: {record.ActualMaterialUsage}.");
                    material.StockQuantity -= record.ActualMaterialUsage;
                    material.ActualQuantity = material.StockQuantity;
                    _context.Materials.Update(material);
                }

                Debug.WriteLine($"Saving ProductionRecord: ProductId={record.ProductId}, MaterialId={record.MaterialId}, Quantity={record.Quantity}, ActualMaterialUsage={record.ActualMaterialUsage}");
                _context.ProductionRecords.Add(record);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine($"DbUpdateException в SaveProduction: {ex.InnerException?.Message}");
                throw new Exception("Ошибка при сохранении записи производства. Проверьте корректность данных.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка в SaveProduction: {ex.Message}");
                throw new Exception("Неизвестная ошибка при сохранении записи производства.", ex);
            }
        }

        public List<Material> GetMaterialStockReport()
        {
            return _context.Materials
                .Include(m => m.InventoryRecords)
                .ToList();
        }

        public List<MaterialMovement> GetMaterialMovementReport(DateTime startDate, DateTime endDate)
        {
            var movements = new List<MaterialMovement>();
            var productionRecords = _context.ProductionRecords
                .Include(pr => pr.Material)
                .Include(pr => pr.Product)
                .Where(pr => pr.ProductionDate >= startDate && pr.ProductionDate <= endDate)
                .ToList();

            if (!productionRecords.Any())
            {
                Debug.WriteLine($"Нет записей в ProductionRecords за период с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}");
            }

            var groupedRecords = productionRecords
                .GroupBy(pr => new { pr.MaterialId, MaterialName = pr.Material.Name, ProductName = pr.Product.Name })
                .Select(g => new MaterialMovement
                {
                    MaterialName = g.Key.MaterialName,
                    ProductName = g.Key.ProductName,
                    InitialStock = 0,
                    Receipts = 0,
                    Issues = g.Any() ? g.Sum(pr => pr.ActualMaterialUsage) : 0, 
                    FinalStock = 0
                })
                .ToList();

            var materials = _context.Materials.ToList();
            foreach (var movement in groupedRecords)
            {
                var material = materials.FirstOrDefault(m => m.Name == movement.MaterialName);
                if (material != null)
                {
                    var totalUsageBeforePeriod = _context.ProductionRecords
                        .Where(pr => pr.MaterialId == material.Id && pr.ProductionDate < startDate)
                        .Sum(pr => pr.ActualMaterialUsage);
                    movement.InitialStock = material.StockQuantity + totalUsageBeforePeriod;

                    var totalUsageInPeriod = _context.ProductionRecords
                        .Where(pr => pr.MaterialId == material.Id && pr.ProductionDate >= startDate && pr.ProductionDate <= endDate)
                        .Sum(pr => pr.ActualMaterialUsage);
                    movement.FinalStock = material.StockQuantity - totalUsageInPeriod + movement.InitialStock - material.StockQuantity;
                }
                Debug.WriteLine($"Movement: {movement.MaterialName}, Product: {movement.ProductName}, Issues: {movement.Issues}, InitialStock: {movement.InitialStock}, FinalStock: {movement.FinalStock}");
            }

            foreach (var material in materials)
            {
                if (!groupedRecords.Any(m => m.MaterialName == material.Name))
                {
                    movements.Add(new MaterialMovement
                    {
                        MaterialName = material.Name,
                        ProductName = "Нет производства",
                        InitialStock = material.StockQuantity,
                        Receipts = 0,
                        Issues = 0,
                        FinalStock = material.StockQuantity
                    });
                }
            }

            movements.AddRange(groupedRecords);
            return movements.OrderBy(m => m.MaterialName).ThenBy(m => m.ProductName).ToList();
        }
    }

    public class MaterialMovement
    {
        public string MaterialName { get; set; }
        public string ProductName { get; set; }
        public double InitialStock { get; set; }
        public double Receipts { get; set; }
        public double Issues { get; set; }
        public double FinalStock { get; set; }
    }
}