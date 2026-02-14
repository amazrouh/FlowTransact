using AutoFixture;
using Payments.Domain.Aggregates;
using Payments.Domain.Enums;
using Shouldly;
using Xunit;

namespace Payments.Domain.UnitTests.Aggregates;

public class PaymentTests
{
    private readonly Fixture _fixture;

    public PaymentTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void Constructor_WithValidData_ShouldCreatePendingPayment()
    {
        var transactionId = _fixture.Create<Guid>();
        var customerId = _fixture.Create<Guid>();
        var amount = 99.99m;

        var payment = new Payment(transactionId, customerId, amount);

        payment.Id.ShouldNotBe(Guid.Empty);
        payment.TransactionId.ShouldBe(transactionId);
        payment.CustomerId.ShouldBe(customerId);
        payment.Amount.ShouldBe(amount);
        payment.Status.ShouldBe(PaymentStatus.Pending);
        payment.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(1));
        payment.CompletedAt.ShouldBeNull();
        payment.FailureReason.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithEmptyTransactionId_ShouldThrowArgumentException()
    {
        var ex = Should.Throw<ArgumentException>(() =>
            new Payment(Guid.Empty, _fixture.Create<Guid>(), 10.00m));
        ex.ParamName.ShouldBe("transactionId");
        ex.Message.ShouldContain("TransactionId cannot be empty");
    }

    [Fact]
    public void Constructor_WithEmptyCustomerId_ShouldThrowArgumentException()
    {
        var ex = Should.Throw<ArgumentException>(() =>
            new Payment(_fixture.Create<Guid>(), Guid.Empty, 10.00m));
        ex.ParamName.ShouldBe("customerId");
        ex.Message.ShouldContain("CustomerId cannot be empty");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Constructor_WithInvalidAmount_ShouldThrowArgumentException(decimal invalidAmount)
    {
        var ex = Should.Throw<ArgumentException>(() =>
            new Payment(_fixture.Create<Guid>(), _fixture.Create<Guid>(), invalidAmount));
        ex.ParamName.ShouldBe("amount");
        ex.Message.ShouldContain("Amount must be positive");
    }

    [Fact]
    public void Confirm_WhenPending_ShouldChangeToCompleted()
    {
        var payment = new Payment(_fixture.Create<Guid>(), _fixture.Create<Guid>(), 50.00m);

        payment.Confirm();

        payment.Status.ShouldBe(PaymentStatus.Completed);
        payment.CompletedAt.ShouldNotBeNull();
        payment.CompletedAt.Value.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Confirm_WhenAlreadyCompleted_ShouldThrowInvalidOperationException()
    {
        var payment = new Payment(_fixture.Create<Guid>(), _fixture.Create<Guid>(), 50.00m);
        payment.Confirm();

        Should.Throw<InvalidOperationException>(() => payment.Confirm())
            .Message.ShouldBe("Only pending payments can be confirmed");
    }

    [Fact]
    public void Confirm_WhenFailed_ShouldThrowInvalidOperationException()
    {
        var payment = new Payment(_fixture.Create<Guid>(), _fixture.Create<Guid>(), 50.00m);
        payment.Fail("Card declined");

        Should.Throw<InvalidOperationException>(() => payment.Confirm())
            .Message.ShouldBe("Only pending payments can be confirmed");
    }

    [Fact]
    public void Fail_WhenPending_WithValidReason_ShouldChangeToFailed()
    {
        var payment = new Payment(_fixture.Create<Guid>(), _fixture.Create<Guid>(), 25.00m);
        var reason = "Card declined";

        payment.Fail(reason);

        payment.Status.ShouldBe(PaymentStatus.Failed);
        payment.FailureReason.ShouldBe(reason);
        payment.CompletedAt.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Fail_WithEmptyReason_ShouldThrowArgumentException(string? invalidReason)
    {
        var payment = new Payment(_fixture.Create<Guid>(), _fixture.Create<Guid>(), 25.00m);

        var ex = Should.Throw<ArgumentException>(() => payment.Fail(invalidReason!));
        ex.ParamName.ShouldBe("reason");
        ex.Message.ShouldContain("Failure reason cannot be empty");
    }

    [Fact]
    public void Fail_WhenAlreadyCompleted_ShouldThrowInvalidOperationException()
    {
        var payment = new Payment(_fixture.Create<Guid>(), _fixture.Create<Guid>(), 25.00m);
        payment.Confirm();

        Should.Throw<InvalidOperationException>(() => payment.Fail("Too late"))
            .Message.ShouldBe("Only pending payments can be failed");
    }

    [Fact]
    public void Fail_WhenAlreadyFailed_ShouldThrowInvalidOperationException()
    {
        var payment = new Payment(_fixture.Create<Guid>(), _fixture.Create<Guid>(), 25.00m);
        payment.Fail("First failure");

        Should.Throw<InvalidOperationException>(() => payment.Fail("Second attempt"))
            .Message.ShouldBe("Only pending payments can be failed");
    }
}
