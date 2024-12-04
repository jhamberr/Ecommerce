using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Models
{
    public class Order
    {

        public int Id { get; set; }
        public string ClientId { get; set; } = "";//foreign key referring to application user in the users table
        public ApplicationUser Client { get; set; } = null!;
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        [Precision(16, 2)]
        public decimal ShippingFee { get; set; }
        public string DeliveryAddress { get; set; } = "";
        public string PaymentMethod { get; set; } = "";
        public string PaymentStatus { get; set; } = "";
        public string PaymentDetails { get; set; } = "";//store paypal details
        public string OrderStatus { get; set; } = "";
        public DateTime CreatedAt { get; set; }

    }
}
