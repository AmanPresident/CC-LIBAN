using System.ComponentModel.DataAnnotations;

namespace test7.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Le mot de passe actuel est requis")]
        [Display(Name = "Mot de passe actuel")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        [Display(Name = "Nouveau mot de passe")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
        [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas")]
        [Display(Name = "Confirmer le nouveau mot de passe")]
        public string ConfirmPassword { get; set; }
    }
}