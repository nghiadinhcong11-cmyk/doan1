using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Application.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync(string? category);
        Task<List<Product>> FindProductsByExactNameAsync(string productName);
        Task<(bool Success, string Message, decimal OldPrice, decimal NewPrice)> UpdatePriceByNameAsync(string productName, decimal newPrice);
        Task<(bool Success, string Message, decimal OldPrice, decimal NewPrice)> UpdatePriceByIdAsync(
            Guid productId,
            decimal expectedCurrentPrice,
            decimal newPrice);
    }
}
