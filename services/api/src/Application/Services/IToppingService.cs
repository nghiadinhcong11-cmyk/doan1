using RestaurantPOS.Application.DTOs.Catalog;
using RestaurantPOS.Domain.Entities;

namespace RestaurantPOS.Application.Services;

public interface IToppingService
{
    Task<(bool Success, string Message, Topping? Topping)> CreateToppingAsync(CreateToppingRequest request);
}
