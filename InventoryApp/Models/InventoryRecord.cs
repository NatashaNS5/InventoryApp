using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryApp.Models
{
    public class InventoryRecord
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int MaterialId { get; set; }
        [Required]
        public double ActualQuantity { get; set; }
        [Required]
        public DateTime InventoryDate { get; set; }
        public bool IsApproved { get; set; }
        [ForeignKey("MaterialId")]
        public Material Material { get; set; }
    }
}
