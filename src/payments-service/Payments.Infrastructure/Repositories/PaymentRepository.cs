using Microsoft.EntityFrameworkCore;
using Npgsql;
using Payments.Application;
using Payments.Application.Exceptions;
using Payments.Domain.Aggregates;
using Payments.Infrastructure.Persistence;

namespace Payments.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentsDbContext _context;

    public PaymentRepository(PaymentsDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Payment?> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);
    }

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Payments.AddAsync(payment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex, payment.TransactionId))
        {
            throw new DuplicatePaymentException(payment.TransactionId);
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex, Guid _)
    {
        return ex.InnerException is PostgresException pg && pg.SqlState == "23505";
    }

    public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
