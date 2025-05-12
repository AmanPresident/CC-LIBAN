using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using test7.Models;

namespace test7.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Produit Product { get; set; }

        public int Quantity { get; set; }

        public int CartId { get; set; }

        [ForeignKey("CartId")]
        public Cart Cart { get; set; }
    }
    
}
