namespace Ecommerce.Application.Discounts
{
    public class RegularCustomerDiscount : IDiscountStrategy
    {
        public decimal CalculateDiscount(decimal totalAmount)
        {
            return totalAmount >= 100 ? totalAmount * 0.05m : 0;
        }
    }
}
