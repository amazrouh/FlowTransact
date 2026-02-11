using Microsoft.EntityFrameworkCore;
using Transactions.Infrastructure.Persistence;

namespace Transactions.IntegrationTests.Fixtures;

public class DatabaseFixture
{
    public TransactionsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TransactionsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new TransactionsDbContext(options);

        // Ensure the database is created
        context.Database.EnsureCreated();

        return context;
    }

    public void ResetDatabase(TransactionsDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
}