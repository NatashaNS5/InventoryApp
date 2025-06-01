using InventoryApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Material> Materials { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<InventoryRecord> InventoryRecords { get; set; }
        public DbSet<ProductionRecord> ProductionRecords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source=inventory.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryRecord>()
                .HasOne(ir => ir.Material)
                .WithMany(m => m.InventoryRecords)
                .HasForeignKey(ir => ir.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductionRecord>()
                .HasOne(pr => pr.Product)
                .WithMany()
                .HasForeignKey(pr => pr.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductionRecord>()
                .HasOne(pr => pr.Material)
                .WithMany()
                .HasForeignKey(pr => pr.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}