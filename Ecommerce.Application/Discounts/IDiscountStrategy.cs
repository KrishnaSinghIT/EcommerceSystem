namespace Ecommerce.Application.Discounts
{
    public interface IDiscountStrategy
    {
        decimal CalculateDiscount(decimal totalAmount);
    }
}
