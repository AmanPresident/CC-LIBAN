using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace test7.Models
{
    public class Produit
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        public string Name { get; set; }

        public string Description { get; set; }

        public string Type { get; set; }

        public double ? Prix { get; set; }
        // Relation avec la catégorie
        public int? CategorieId { get; set; }
        public Categorie? Categorie { get; set; }



        public string? UrlImage { get; set; } 

        [NotMapped] 
        public IFormFile? ImageFile { get; set; }
    }
}
