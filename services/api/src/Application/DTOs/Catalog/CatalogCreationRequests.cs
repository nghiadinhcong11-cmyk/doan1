namespace RestaurantPOS.Application.DTOs.Catalog;

public sealed record CreateProductRequest(
    string Name,
    decimal Price,
    decimal CostPrice,
    string Category,
    string Group,
    bool IsActive = true);

public sealed record CreateToppingRequest(
    string Name,
    decimal Price,
    decimal CostPrice,
    string ToppingType,
    string Category,
    string? Description,
    bool IsActive = true);
