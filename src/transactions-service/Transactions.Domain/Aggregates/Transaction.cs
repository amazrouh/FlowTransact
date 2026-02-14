using Transactions.Domain.Entities;
using Transactions.Domain.Enums;
using Transactions.Domain.Events;

namespace Transactions.Domain.Aggregates;

public class Transaction : IHasDomainEvents
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public TransactionStatus Status { get; private set; }
    public decimal TotalAmount => Items.Sum(item => item.TotalPrice);
    public DateTime CreatedAt { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int Version { get; private set; } // For optimistic concurrency

    private readonly List<TransactionItem> _items = new();
    public IReadOnlyCollection<TransactionItem> Items => _items.AsReadOnly();

    private List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Transaction() { } // EF Core constructor

    public Transaction(Guid customerId)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        Status = TransactionStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        Version = 1;
    }

    public void AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (Status != TransactionStatus.Draft)
            throw new InvalidOperationException("Items can only be added to draft transactions");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (unitPrice <= 0)
            throw new ArgumentException("Unit price must be positive", nameof(unitPrice));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("ProductName cannot be empty or whitespace", nameof(productName));

        var item = new TransactionItem(Id, productId, productName, quantity, unitPrice);
        _items.Add(item);

        // Raise domain event
        var itemAdded = new TransactionItemAddedDomainEvent(Id, item.Id, productId, productName, quantity, unitPrice);
        _domainEvents.Add(itemAdded);
    }

    public void Submit()
    {
        if (Status != TransactionStatus.Draft)
            throw new InvalidOperationException("Only draft transactions can be submitted");

        if (!_items.Any())
            throw new InvalidOperationException("Cannot submit transaction without items");

        // Validate aggregate invariant: transaction must have positive total amount
        if (TotalAmount <= 0)
            throw new InvalidOperationException("Transaction total amount must be positive");

        Status = TransactionStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;

        // Add domain event
        var transactionSubmitted = new TransactionSubmittedDomainEvent(Id, CustomerId, TotalAmount);
        _domainEvents.Add(transactionSubmitted);
    }

    public void Cancel()
    {
        if (Status != TransactionStatus.Draft && Status != TransactionStatus.Submitted)
            throw new InvalidOperationException("Only draft or submitted transactions can be cancelled");

        Status = TransactionStatus.Cancelled;
    }

    public void MarkAsCompleted()
    {
        if (Status != TransactionStatus.Submitted)
            throw new InvalidOperationException("Only submitted transactions can be completed");

        Status = TransactionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}