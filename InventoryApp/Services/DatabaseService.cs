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
            _context.ProductionRecords.Add(record);
            _context.SaveChanges();
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
            var materials = _context.Materials
                .Include(m => m.InventoryRecords)
                .ToList();

            foreach (var material in materials)
            {
                var records = material.InventoryRecords
                    .Where(ir => ir.InventoryDate >= startDate && ir.InventoryDate <= endDate)
                    .ToList();

                var movement = new MaterialMovement
                {
                    MaterialName = material.Name,
                    InitialStock = material.StockQuantity,
                    Receipts = 0,
                    Issues = records.Any() ? Math.Max(0, material.ActualQuantity - material.StockQuantity) : 0,
                    FinalStock = material.StockQuantity
                };
                movements.Add(movement);
            }
            return movements;
        }
    }

    public class MaterialMovement
    {
        public string MaterialName { get; set; }
        public double InitialStock { get; set; }
        public double Receipts { get; set; }
        public double Issues { get; set; }
        public double FinalStock { get; set; }
    }
}