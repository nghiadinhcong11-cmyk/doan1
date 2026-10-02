namespace RestaurantPOS.Application.Services;

public sealed class InventoryValidationException(string message) : Exception(message);

public sealed class InventoryNotFoundException(string message) : Exception(message);

public sealed class InventoryForbiddenException(string message) : Exception(message);

public sealed class InventoryConflictException(string message) : Exception(message);
