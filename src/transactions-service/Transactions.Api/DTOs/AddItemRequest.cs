namespace Transactions.Api.DTOs;

public record AddItemRequest(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);