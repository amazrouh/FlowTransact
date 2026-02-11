namespace Transactions.Domain.Entities;

public class TransactionItem
{
    public Guid Id { get; private set; }
    public Guid TransactionId { get; private set; } // Foreign key to Transaction
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    private TransactionItem()
    {
        ProductName = string.Empty; // EF Core constructor
    }

    public TransactionItem(Guid transactionId, Guid productId, string productName, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        TransactionId = transactionId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    // Business methods if needed
}