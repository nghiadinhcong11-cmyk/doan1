using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Application.DTOs.Catalog;

namespace RestaurantPOS.Application.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync(string? category, Guid? branchId = null);
        Task<List<Product>> GetSellableProductsAsync(string? category, Guid branchId);
        Task<List<Product>> FindProductsByExactNameAsync(string productName);
        Task<(bool Success, string Message, decimal OldPrice, decimal NewPrice)> UpdatePriceByNameAsync(string productName, decimal newPrice);
        Task<(bool Success, string Message, decimal OldPrice, decimal NewPrice)> UpdatePriceByIdAsync(
            Guid productId,
            decimal expectedCurrentPrice,
            decimal newPrice);
        Task<(bool Success, string Message, Product? Product)> CreateProductAsync(CreateProductRequest request);
        Task<(bool Success, string Message, string OldName, string NewName)> UpdateNameByIdAsync(
            Guid productId,
            string expectedCurrentName,
            string newName);
    }
}
