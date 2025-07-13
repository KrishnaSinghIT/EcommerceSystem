namespace Ecommerce.Application.Discounts
{
    public class PremiumCustomerDiscount : IDiscountStrategy
    {
        public decimal CalculateDiscount(decimal totalAmount)
        {
            return totalAmount * 0.10m;
        }
    }
}
