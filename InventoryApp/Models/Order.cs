using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryApp.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public string Customer { get; set; }
        public string? Manager { get; set; }
        public double TotalCost { get; set; }
        public int TotalItems { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>(); 
    }
}
