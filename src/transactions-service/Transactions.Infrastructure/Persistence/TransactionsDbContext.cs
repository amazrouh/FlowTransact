using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Transactions.Domain.Aggregates;
using Transactions.Domain.Entities;

namespace Transactions.Infrastructure.Persistence;

public class TransactionsDbContext : DbContext
{
    public TransactionsDbContext(DbContextOptions<TransactionsDbContext> options)
        : base(options)
    {
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
            entity.Property(t => t.UnitPrice).HasPrecision(18, 2);
            entity.Ignore(t => t.TotalPrice); // Computed property, not stored in DB
        });

        // Add MassTransit outbox configuration
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }

    public async Task PublishDomainEventsAsync(IPublishEndpoint? publishEndpoint, CancellationToken cancellationToken = default)
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
                // Set correlation ID on the domain event if it supports it
                if (domainEvent is MoneyFellows.Contracts.Events.DomainEvent baseEvent)
                {
                    // Create a new event instance with correlation ID
                    var eventWithCorrelationId = baseEvent with { CorrelationId = correlationId };
                    await publishEndpoint.Publish(eventWithCorrelationId, cancellationToken);
                }
                else
                {
                    await publishEndpoint.Publish(domainEvent, cancellationToken);
                }
            }
        }
    }
}