using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace test7.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? Password { get; set; } // Pour BCrypt (utilisé dans ChangePassword)
        public string? Statut { get; set; }
        public string? Role { get; set; } // Propriété utilisée dans la vue

        // Propriétés manquantes pour le profil
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? ProfileImage { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Email confirmation
        public bool IsEmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }
        public DateTime? TokenCreatedAt { get; set; }

        // Propriété pour upload d'image (non mappée en base)
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}