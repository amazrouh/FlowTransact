using System.ComponentModel.DataAnnotations;

namespace Transactions.Api.DTOs;

public record CreateTransactionRequest(
    [Required(ErrorMessage = "CustomerId is required")]
    Guid CustomerId);