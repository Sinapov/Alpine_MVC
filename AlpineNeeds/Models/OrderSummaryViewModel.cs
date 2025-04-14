namespace AlpineNeeds.Models
{
    public class OrderSummaryViewModel
    {
        public decimal Subtotal { get; set; }
        public decimal Shipping { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        // Add properties for items, etc.
    }
}
