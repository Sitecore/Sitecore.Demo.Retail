namespace Sitecore.Feature.Commerce.Orders.Website.Models
{
    public class PaymentInfoViewModel
    {
        public PaymentType PaymentType { get; set; }
        public string CustomerName { get; set; }
        public string CardType { get; set; }
        public string CreditCardNumber { get; set; }
        public int ExpirationMonth { get; set; }
        public int ExpirationYear { get; set; }
        public decimal Amount { get; set; }
        public string GiftCardId { get; set; }
    }
}