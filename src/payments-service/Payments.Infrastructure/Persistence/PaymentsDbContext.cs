using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Aggregates;
using Payments.Domain.Enums;

namespace Payments.Infrastructure.Persistence;

public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Status).HasConversion<string>();
            entity.Property(p => p.Amount).HasPrecision(18, 2);
            entity.HasIndex(p => p.TransactionId).IsUnique();
        });

        // MassTransit inbox/outbox for idempotent consumers (TransactionSubmitted)
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
