using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.Factories
{
    public static class ProductTypeProcessorFactory
    {
        public static IProductTypeProcessor Create(ProductType type)
        {
            return type switch
            {
                ProductType.Digital => new DigitalProductProcessor(),
                ProductType.Physical => new PhysicalProductProcessor(),
                _ => throw new NotImplementedException()
            };
        }
    }
    public interface IProductTypeProcessor
    {
        Task ProcessAsync(); // in real case, might take OrderItem or Product
    }
    public class DigitalProductProcessor : IProductTypeProcessor
    {
        public Task ProcessAsync()
        {
            // Generate license key logic
            Console.WriteLine("License key generated.");
            return Task.CompletedTask;
        }
    }
    public class PhysicalProductProcessor : IProductTypeProcessor
    {
        public Task ProcessAsync()
        {
            // Prepare for shipping, etc.
            Console.WriteLine("Physical product prepared for shipment.");
            return Task.CompletedTask;
        }
    }
}
