using System.ComponentModel.DataAnnotations;

namespace test7.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "L'adresse de livraison est obligatoire")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le numéro de téléphone est obligatoire")]
        [Phone(ErrorMessage = "Format de téléphone invalide")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string ?  Notes { get; set; } = string.Empty;

        // Informations du panier (en lecture seule)
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; } = 500; // 500 Fdj par défaut
        public decimal Total { get; set; }
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }

    

    
}