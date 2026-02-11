using System.ComponentModel.DataAnnotations;

namespace Transactions.Api.DTOs;

public record AddItemRequest(
    [Required(ErrorMessage = "ProductId is required")]
    Guid ProductId,

    [Required(ErrorMessage = "ProductName is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "ProductName must be between 1 and 200 characters")]
    string ProductName,

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    int Quantity,

    [Range(0.01, double.MaxValue, ErrorMessage = "UnitPrice must be greater than 0")]
    decimal UnitPrice);