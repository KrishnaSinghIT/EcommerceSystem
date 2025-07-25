﻿namespace Ecommerce.Application.DTOs.Product
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ProductType { get; set; } = string.Empty;
        public int InventoryCount { get; set; }
    }
}
