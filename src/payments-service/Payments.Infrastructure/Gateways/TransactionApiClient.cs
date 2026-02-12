using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Payments.Application;

namespace Payments.Infrastructure.Gateways;

public class TransactionApiClient : ITransactionApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TransactionApiClient(HttpClient httpClient, IOptions<TransactionApiOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl.TrimEnd('/'));
    }

    public async Task<TransactionInfo?> GetTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/transactions/{transactionId}", cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<TransactionResponseDto>(_jsonOptions, cancellationToken);
        return dto is null ? null : new TransactionInfo(dto.CustomerId, dto.TotalAmount, dto.Status);
    }

    private record TransactionResponseDto(Guid CustomerId, decimal TotalAmount, string Status);
}

public class TransactionApiOptions
{
    public const string SectionName = "TransactionApi";
    public string BaseUrl { get; set; } = "http://localhost:5263";
}
