using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryApp.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public double StockQuantity { get; set; }
        public double ActualQuantity { get; set; }
        public double PurchasePrice { get; set; }
        public double DiscrepancyPercentage { get; set; }

        public List<InventoryRecord> InventoryRecords { get; set; } = new List<InventoryRecord>();
    }
}
