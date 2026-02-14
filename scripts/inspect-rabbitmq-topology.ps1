# Inspect RabbitMQ topology for TransactionSubmitted debugging
# Requires: RabbitMQ Management plugin (default on rabbitmq:3-management image)
# Usage: .\inspect-rabbitmq-topology.ps1
# Or: .\inspect-rabbitmq-topology.ps1 -BaseUrl "http://localhost:15672" -User "guest" -Password "guest"

param(
    [string]$BaseUrl = "http://localhost:15672",
    [string]$User = "guest",
    [string]$Password = "guest"
)

$cred = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${User}:${Password}"))
$headers = @{ Authorization = "Basic $cred" }

Write-Host "`n=== RabbitMQ Topology for TransactionSubmitted ===" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl`n" -ForegroundColor Gray

# Exchanges to check
$exchanges = @(
    "MoneyFellows.Contracts.Events:TransactionSubmitted",
    "MoneyFellows.Contracts.Events:PaymentConfirmed",
    "transaction-submitted"
)

Write-Host "--- Exchanges ---" -ForegroundColor Yellow
try {
    $allExchanges = Invoke-RestMethod -Uri "$BaseUrl/api/exchanges/%2F" -Headers $headers
    foreach ($name in $exchanges) {
        $ex = $allExchanges | Where-Object { $_.name -eq $name }
        if ($ex) {
            Write-Host "  [EXISTS] $name (type: $($ex.type))" -ForegroundColor Green
        } else {
            Write-Host "  [MISSING] $name" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "  Error: $_" -ForegroundColor Red
}

# Queue transaction-submitted and its bindings
Write-Host "`n--- Queue: transaction-submitted ---" -ForegroundColor Yellow
try {
    $queue = Invoke-RestMethod -Uri "$BaseUrl/api/queues/%2F/transaction-submitted" -Headers $headers
    Write-Host "  Messages: $($queue.messages) | Consumers: $($queue.consumers)" -ForegroundColor Gray
    $bindings = Invoke-RestMethod -Uri "$BaseUrl/api/queues/%2F/transaction-submitted/bindings" -Headers $headers
    Write-Host "  Bindings:" -ForegroundColor Gray
    foreach ($b in $bindings) {
        if ($b.source -ne "") {
            Write-Host "    -> from exchange: $($b.source)" -ForegroundColor $(if ($b.source -eq "MoneyFellows.Contracts.Events:TransactionSubmitted") { "Green" } else { "Yellow" })
        }
    }
} catch {
    Write-Host "  Queue not found or error: $_" -ForegroundColor Red
}

Write-Host "`n--- Expected ---" -ForegroundColor Yellow
Write-Host "  transaction-submitted queue should bind to: MoneyFellows.Contracts.Events:TransactionSubmitted" -ForegroundColor Gray
Write-Host "  If binding is missing or different, Publish will not reach the Payments consumer.`n" -ForegroundColor Gray
