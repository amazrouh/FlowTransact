using Microsoft.EntityFrameworkCore;
using Transactions.Infrastructure.Persistence;

namespace Transactions.IntegrationTests.Fixtures;

public class DatabaseFixture
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    public TransactionsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TransactionsDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
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