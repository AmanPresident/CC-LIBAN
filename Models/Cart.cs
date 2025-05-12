using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using test7.Models;

namespace test7.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; } // Lien vers l'utilisateur connecté

        [ForeignKey("UserId")]
        public User User { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
