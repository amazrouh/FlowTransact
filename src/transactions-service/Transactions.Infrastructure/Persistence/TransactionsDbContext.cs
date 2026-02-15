using MassTransit;
using Microsoft.EntityFrameworkCore;
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
}