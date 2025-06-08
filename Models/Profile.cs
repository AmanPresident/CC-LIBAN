namespace test7.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Adresse { get; set; }
        
        public string Ville { get; set; }
        public string CodePostal { get; set; }
        public string Pays { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public DateTime DateNaissance { get; set; }
        public string Sexe { get; set; } // "Homme" ou "Femme"
        public User User { get; set; } // Lien vers l'utilisateur connecté
        public string PhotoProfil { get; set; } // URL de la photo de profil
    }
}
