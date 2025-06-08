namespace test7.ViewModels
{
    // AJOUT MANQUANT: OrderConfirmationViewModel
    public class OrderConfirmationViewModel
    {
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public List<CartItemViewModel> OrderItems { get; set; } = new List<CartItemViewModel>();
    }
}