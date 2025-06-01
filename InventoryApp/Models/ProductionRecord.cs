using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryApp.Models
{
    public class ProductionRecord
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public double ActualMaterialUsage { get; set; }
        [Required]
        public DateTime ProductionDate { get; set; }
        public double ScrapAmount { get; set; }
        [Required]
        public int MaterialId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        [ForeignKey("MaterialId")] 
        public Material Material { get; set; }
    }
}