$base = "http://localhost:5263"
$custId = [guid]::NewGuid()
$prodId = [guid]::NewGuid()

# Create
$r1 = Invoke-RestMethod -Uri "$base/api/transactions" -Method Post -ContentType "application/json" -Body (@{customerId=$custId} | ConvertTo-Json)
$tid = $r1.TransactionId
Write-Host "Created: $tid"

# Add item
$body2 = @{productId=$prodId; productName="Product 1"; quantity=2; unitPrice=10.50} | ConvertTo-Json
Invoke-RestMethod -Uri "$base/api/transactions/$tid/items" -Method Post -ContentType "application/json" -Body $body2 | Out-Null
Write-Host "Added item"

# Submit
Invoke-RestMethod -Uri "$base/api/transactions/$tid/submit" -Method Post | Out-Null
Write-Host "Submitted"

# Verify status
$tx = Invoke-RestMethod -Uri "$base/api/transactions/$tid" -Method Get
Write-Host "Status: $($tx.status)"

# Check outbox
$outbox = Invoke-RestMethod -Uri "$base/api/diagnostics/outbox" -Method Get
Write-Host "Outbox TotalPending: $($outbox.totalPending)"
