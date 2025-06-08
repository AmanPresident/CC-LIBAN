using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test7.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string OrderNumber { get; set; } // Numéro de commande généré

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? PaymentDate { get; set; }
        public DateTime? ValidationDate { get; set; }

        // Informations de livraison
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Notes { get; set; }

        // Relations
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public enum OrderStatus
    {
        Pending,        // En attente de paiement
        Paid,          // Payé mais non validé
        Validated,     // Validé par l'admin
        Shipped,       // Expédié
        Delivered,     // Livré
        Cancelled      // Annulé
    }
}