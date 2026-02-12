namespace Payments.Application.Exceptions;

public class CustomerMismatchException : Exception
{
    public CustomerMismatchException(string message) : base(message) { }
}
