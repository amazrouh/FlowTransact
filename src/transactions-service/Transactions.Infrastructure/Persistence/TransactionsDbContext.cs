using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Transactions.Domain.Aggregates;
using Transactions.Domain.Entities;
using Transactions.Domain.Events;
using MoneyFellows.Contracts.Events;

namespace Transactions.Infrastructure.Persistence;

public class TransactionsDbContext : DbContext
{
    private readonly IPublishEndpoint? _publishEndpoint;

    public TransactionsDbContext(DbContextOptions<TransactionsDbContext> options, IPublishEndpoint? publishEndpoint = null)
        : base(options)
    {
        _publishEndpoint = publishEndpoint;
    }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionItem> TransactionItems => Set<TransactionItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Transaction aggregate
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).ValueGeneratedNever();
            entity.Property(t => t.Status).HasConversion<string>();
            entity.Ignore(t => t.TotalAmount); // Computed property, not stored in DB
            entity.Ignore(t => t.Version); // Temporarily ignore to avoid concurrency issues

            // Configure one-to-many relationship with TransactionItem
            entity.HasMany(t => t.Items)
                .WithOne()
                .HasForeignKey("TransactionId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TransactionItem entity
        modelBuilder.Entity<TransactionItem>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).ValueGeneratedNever();
            entity.Property(t => t.UnitPrice).HasPrecision(18, 2);
            entity.Ignore(t => t.TotalPrice); // Computed property, not stored in DB
        });

        // MassTransit inbox/outbox for idempotent consumers (PaymentConfirmed, PaymentFailed)
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Publish BEFORE SaveChanges - required for MassTransit UseBusOutbox.
        // Publishing during SaveChanges interceptor can deadlock with outbox infrastructure.
        await PublishDomainEventsAsync(_publishEndpoint, null, cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task PublishDomainEventsAsync(IPublishEndpoint? publishEndpoint, ISendEndpointProvider? sendEndpointProvider, CancellationToken cancellationToken = default)
    {
        if (publishEndpoint is null)
            return;

        // Get correlation ID from current activity or HTTP context
        var correlationId = Activity.Current?.GetTagItem("correlation.id") as string ??
                           Guid.NewGuid().ToString("N");

        var entities = ChangeTracker
            .Entries()
            .Where(e => e.Entity is Transactions.Domain.IHasDomainEvents)
            .Select(e => e.Entity as Transactions.Domain.IHasDomainEvents)
            .Where(e => e?.DomainEvents.Any() == true)
            .ToList();

        foreach (var entity in entities)
        {
            if (entity is null) continue;

            var events = entity.DomainEvents.ToList();
            entity.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                object? eventToSend = domainEvent switch
                {
                    TransactionSubmittedDomainEvent e => new TransactionSubmitted(e.TransactionId, e.CustomerId, e.TotalAmount)
                    {
                        CorrelationId = correlationId
                    },
                    TransactionItemAddedDomainEvent e => new TransactionItemAdded(e.TransactionId, e.ItemId, e.ProductId, e.ProductName, e.Quantity, e.UnitPrice)
                    {
                        CorrelationId = correlationId
                    },
                    _ => null
                };

                if (eventToSend is not null)
                    await publishEndpoint.Publish(eventToSend, cancellationToken);
            }
        }
    }
}