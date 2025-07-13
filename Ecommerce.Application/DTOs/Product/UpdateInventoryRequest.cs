namespace Ecommerce.Application.DTOs.Product
{
    public class UpdateInventoryRequest
    {
        public int ProductId { get; set; }
        public int QuantityToAdd { get; set; }
    }
}
