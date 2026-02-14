namespace Transactions.Application.Queries;

public record TransactionDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    DateTime? CompletedAt,
    List<TransactionItemDto> Items);

public record TransactionItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);
