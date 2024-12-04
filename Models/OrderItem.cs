using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{

    [Table("OrderItems")]//specify name of table for the DB
    public class OrderItem
    {
        
        public int Id { get; set; }
        public int Quantity { get; set; }
        [Precision(16, 2)]
        public decimal UnitPrice { get; set; }
        //navigation property
        public Product Product { get; set; } = new Product();
    }
}
