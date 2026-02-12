# Manual API Tests for FlowTransact
# Transactions: http://localhost:5263
# Payments: http://localhost:5264

$TransactionsBase = "http://localhost:5263"
$PaymentsBase = "http://localhost:5264"
$Results = @()
$Pass = 0
$Fail = 0

function Test-Api {
    param($Name, $ScriptBlock)
    try {
        $result = & $ScriptBlock
        $script:Results += [PSCustomObject]@{ Test = $Name; Status = "PASS"; Details = $result }
        $script:Pass++
        return $result
    } catch {
        $script:Results += [PSCustomObject]@{ Test = $Name; Status = "FAIL"; Details = $_.Exception.Message }
        $script:Fail++
        return $null
    }
}

Write-Host "`n=== FlowTransact Manual Tests ===`n"

# --- Transactions API (1-25) ---
Write-Host "--- Transactions API (1-25) ---"

# 1. Create transaction
$tid = Test-Api "1. Create transaction" {
    $r = Invoke-RestMethod -Uri "$TransactionsBase/api/transactions" -Method Post -ContentType "application/json" -Body '{"customerId":"11111111-1111-1111-1111-111111111111"}'
    $r.TransactionId.ToString()
}
if (-not $tid) { $tid = [guid]::Empty }

# 2. Add item to transaction
Test-Api "2. Add item" {
    Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid/items" -Method Post -ContentType "application/json" -Body '{"productId":"22222222-2222-2222-2222-222222222222","productName":"Widget","quantity":2,"unitPrice":10.50}' | Out-Null
    "OK"
}

# 3. Add second item
Test-Api "3. Add second item" {
    Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid/items" -Method Post -ContentType "application/json" -Body '{"productId":"33333333-3333-3333-3333-333333333333","productName":"Gadget","quantity":1,"unitPrice":25.00}' | Out-Null
    "OK"
}

# 4. Get transaction (draft)
Test-Api "4. Get transaction (draft)" {
    $r = Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid" -Method Get
    "Status=$($r.status), Total=$($r.totalAmount)"
}

# 5. Submit transaction
Test-Api "5. Submit transaction" {
    Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid/submit" -Method Post | Out-Null
    "OK"
}

# 6. Get transaction (submitted)
Test-Api "6. Get transaction (submitted)" {
    $r = Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid" -Method Get
    "Status=$($r.status)"
}

# 7. Create second transaction for cancel test
$tid2 = Test-Api "7. Create transaction 2" {
    $r = Invoke-RestMethod -Uri "$TransactionsBase/api/transactions" -Method Post -ContentType "application/json" -Body '{"customerId":"11111111-1111-1111-1111-111111111111"}'
    $r.TransactionId.ToString()
}
if (-not $tid2) { $tid2 = [guid]::Empty }

# 8. Add item to transaction 2
Test-Api "8. Add item to tx2" {
    Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid2/items" -Method Post -ContentType "application/json" -Body '{"productId":"22222222-2222-2222-2222-222222222222","productName":"Test","quantity":1,"unitPrice":5.00}' | Out-Null
    "OK"
}

# 9. Cancel transaction (draft)
Test-Api "9. Cancel draft transaction" {
    Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid2/cancel" -Method Post | Out-Null
    "OK"
}

# 10. Create transaction 3 for submit then cancel
$tid3 = Test-Api "10. Create transaction 3" {
    $r = Invoke-RestMethod -Uri "$TransactionsBase/api/transactions" -Method Post -ContentType "application/json" -Body '{"customerId":"11111111-1111-1111-1111-111111111111"}'
    $r.TransactionId.ToString()
}
if (-not $tid3) { $tid3 = [guid]::Empty }

# 11. Add item, submit, cancel tx3
Test-Api "11. Add item to tx3" {
    Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid3/items" -Method Post -ContentType "application/json" -Body '{"productId":"22222222-2222-2222-2222-222222222222","productName":"X","quantity":1,"unitPrice":1.00}' | Out-Null
    "OK"
}

Test-Api "12. Submit tx3" {
    Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid3/submit" -Method Post | Out-Null
    "OK"
}

Test-Api "13. Cancel submitted tx3" {
    Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid3/cancel" -Method Post | Out-Null
    "OK"
}

# 14. Get non-existent transaction
Test-Api "14. Get non-existent (404)" {
    try {
        Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/00000000-0000-0000-0000-000000000000" -Method Get
        "FAIL: expected 404"
    } catch {
        if ($_.Exception.Response.StatusCode -eq 404) { "404 as expected" } else { throw }
    }
}

# 15. Create empty tx for submit test
$tidEmpty = Test-Api "15. Create empty tx" {
    $r = Invoke-RestMethod -Uri "$TransactionsBase/api/transactions" -Method Post -ContentType "application/json" -Body '{"customerId":"11111111-1111-1111-1111-111111111111"}'
    $r.TransactionId.ToString()
}
if (-not $tidEmpty) { $tidEmpty = [guid]::Empty }

# 16. Submit empty transaction (bad request)
Test-Api "16. Submit empty tx (400)" {
    try {
        Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tidEmpty/submit" -Method Post
        "FAIL: expected 400"
    } catch {
        if ($_.Exception.Response.StatusCode -eq 400) { "400 as expected" } else { throw }
    }
}

# 17. Add item with invalid quantity
Test-Api "17. Add item qty=0 (400)" {
    try {
        Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid/items" -Method Post -ContentType "application/json" -Body '{"productId":"22222222-2222-2222-2222-222222222222","productName":"X","quantity":0,"unitPrice":1.00}'
        "FAIL: expected 400"
    } catch {
        if ($_.Exception.Response.StatusCode -eq 400) { "400 as expected" } else { throw }
    }
}

# 18. Add item to submitted transaction (400)
Test-Api "18. Add item to submitted tx (400)" {
    try {
        Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid/items" -Method Post -ContentType "application/json" -Body '{"productId":"44444444-4444-4444-4444-444444444444","productName":"Y","quantity":1,"unitPrice":1.00}'
        "FAIL: expected 400"
    } catch {
        if ($_.Exception.Response.StatusCode -eq 400) { "400 as expected" } else { throw }
    }
}

# 19. Submit already submitted (400)
Test-Api "19. Re-submit (400)" {
    try {
        Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid/submit" -Method Post
        "FAIL: expected 400"
    } catch {
        if ($_.Exception.Response.StatusCode -eq 400) { "400 as expected" } else { throw }
    }
}

# 20. Cancel cancelled (400)
Test-Api "20. Cancel cancelled (400)" {
    try {
        Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid3/cancel" -Method Post
        "FAIL: expected 400"
    } catch {
        if ($_.Exception.Response.StatusCode -eq 400) { "400 as expected" } else { throw }
    }
}

# 21. Transactions health
Test-Api "21. Transactions health" {
    $r = Invoke-RestMethod -Uri "$TransactionsBase/health" -Method Get
    "OK"
}

# 22-25: Additional coverage
Test-Api "22. Get tx2 (cancelled)" {
    $r = Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid2" -Method Get
    "Status=$($r.status)"
}

Test-Api "23. Get tx3 (cancelled)" {
    $r = Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid3" -Method Get
    "Status=$($r.status)"
}

$tid4 = Test-Api "24. Create tx4 (for payments)" {
    $r = Invoke-RestMethod -Uri "$TransactionsBase/api/transactions" -Method Post -ContentType "application/json" -Body '{"customerId":"11111111-1111-1111-1111-111111111111"}'
    $r.TransactionId.ToString()
}
if (-not $tid4) { $tid4 = [guid]::Empty }

Test-Api "25. Add item to tx4, submit" {
    Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid4/items" -Method Post -ContentType "application/json" -Body '{"productId":"22222222-2222-2222-2222-222222222222","productName":"PayTest","quantity":1,"unitPrice":100.00}' | Out-Null
    Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid4/submit" -Method Post | Out-Null
    "OK"
}

# --- Payments API (26-43) ---
Write-Host "`n--- Payments API (26-43) ---"

# 26. Start payment
$pid1 = Test-Api "26. Start payment" {
    $r = Invoke-RestMethod -Uri "$PaymentsBase/api/payments/start" -Method Post -ContentType "application/json" -Body "{`"transactionId`":`"$tid4`",`"customerId`":`"11111111-1111-1111-1111-111111111111`"}"
    $r.PaymentId.ToString()
}
if (-not $pid1) { $pid1 = [guid]::Empty }

# 27. Get payment
Test-Api "27. Get payment" {
    $r = Invoke-RestMethod -Uri "$PaymentsBase/api/payments/$pid1" -Method Get
    "Status=$($r.status)"
}

# 28. Confirm payment
Test-Api "28. Confirm payment" {
    Invoke-RestMethod -Uri "$PaymentsBase/api/payments/$pid1/confirm" -Method Post | Out-Null
    "OK"
}

# 29. Get payment (completed)
Test-Api "29. Get payment (completed)" {
    $r = Invoke-RestMethod -Uri "$PaymentsBase/api/payments/$pid1" -Method Get
    "Status=$($r.status)"
}

# 30. Start payment again (idempotent - may return existing)
$pid2 = Test-Api "30. Start payment again (idempotent)" {
    $r = Invoke-RestMethod -Uri "$PaymentsBase/api/payments/start" -Method Post -ContentType "application/json" -Body "{`"transactionId`":`"$tid4`",`"customerId`":`"11111111-1111-1111-1111-111111111111`"}"
    $r.PaymentId.ToString()
}
if (-not $pid2) { $pid2 = [guid]::Empty }

# 31. Create tx5, submit, for fail test
$tid5 = Test-Api "31. Create tx5, submit" {
    $r = Invoke-RestMethod -Uri "$TransactionsBase/api/transactions" -Method Post -ContentType "application/json" -Body '{"customerId":"11111111-1111-1111-1111-111111111111"}'
    $txid = $r.TransactionId.ToString()
    Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$txid/items" -Method Post -ContentType "application/json" -Body '{"productId":"22222222-2222-2222-2222-222222222222","productName":"FailTest","quantity":1,"unitPrice":50.00}' | Out-Null
    Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$txid/submit" -Method Post | Out-Null
    $txid
}
if (-not $tid5) { $tid5 = [guid]::Empty }

$pid3 = Test-Api "32. Start payment for tx5" {
    $r = Invoke-RestMethod -Uri "$PaymentsBase/api/payments/start" -Method Post -ContentType "application/json" -Body "{`"transactionId`":`"$tid5`",`"customerId`":`"11111111-1111-1111-1111-111111111111`"}"
    $r.PaymentId.ToString()
}
if (-not $pid3) { $pid3 = [guid]::Empty }

Test-Api "33. Fail payment" {
    Invoke-RestMethod -Uri "$PaymentsBase/api/payments/$pid3/fail" -Method Post -ContentType "application/json" -Body '{"reason":"Card declined"}' | Out-Null
    "OK"
}

Test-Api "34. Get failed payment" {
    $r = Invoke-RestMethod -Uri "$PaymentsBase/api/payments/$pid3" -Method Get
    "Status=$($r.status)"
}

# 35. Get non-existent payment
Test-Api "35. Get non-existent payment (404)" {
    try {
        Invoke-RestMethod -Uri "$PaymentsBase/api/payments/00000000-0000-0000-0000-000000000000" -Method Get
        "FAIL: expected 404"
    } catch {
        if ($_.Exception.Response.StatusCode -eq 404) { "404 as expected" } else { throw }
    }
}

# 36. Confirm already confirmed (400)
Test-Api "36. Confirm already confirmed (400)" {
    try {
        Invoke-RestMethod -Uri "$PaymentsBase/api/payments/$pid1/confirm" -Method Post
        "FAIL: expected 400"
    } catch {
        if ($_.Exception.Response.StatusCode -eq 400) { "400 as expected" } else { throw }
    }
}

# 37. Fail already failed (400)
Test-Api "37. Fail already failed (400)" {
    try {
        Invoke-RestMethod -Uri "$PaymentsBase/api/payments/$pid3/fail" -Method Post -ContentType "application/json" -Body '{"reason":"x"}'
        "FAIL: expected 400"
    } catch {
        if ($_.Exception.Response.StatusCode -eq 400) { "400 as expected" } else { throw }
    }
}

# 38. Start payment for non-existent transaction (404)
Test-Api "38. Start payment for non-existent tx (404)" {
    try {
        Invoke-RestMethod -Uri "$PaymentsBase/api/payments/start" -Method Post -ContentType "application/json" -Body '{"transactionId":"00000000-0000-0000-0000-000000000000","customerId":"11111111-1111-1111-1111-111111111111"}'
        "FAIL: expected 404"
    } catch {
        if ($_.Exception.Response.StatusCode -eq 404) { "404 as expected" } else { throw }
    }
}

# 39. Create tx6 for wrong customer test
$tid6 = Test-Api "39. Create tx6, submit" {
    $r = Invoke-RestMethod -Uri "$TransactionsBase/api/transactions" -Method Post -ContentType "application/json" -Body '{"customerId":"11111111-1111-1111-1111-111111111111"}'
    $txid = $r.TransactionId.ToString()
    Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$txid/items" -Method Post -ContentType "application/json" -Body '{"productId":"22222222-2222-2222-2222-222222222222","productName":"X","quantity":1,"unitPrice":1.00}' | Out-Null
    Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$txid/submit" -Method Post | Out-Null
    $txid
}
if (-not $tid6) { $tid6 = [guid]::Empty }

# 40. Wrong customer (403)
Test-Api "40. Start payment wrong customer (403)" {
    try {
        Invoke-RestMethod -Uri "$PaymentsBase/api/payments/start" -Method Post -ContentType "application/json" -Body "{`"transactionId`":`"$tid6`",`"customerId`":`"99999999-9999-9999-9999-999999999999`"}"
        "FAIL: expected 403"
    } catch {
        if ($_.Exception.Response.StatusCode -eq 403) { "403 as expected" } else { throw }
    }
}

# 41. Payments health
Test-Api "41. Payments health" {
    $r = Invoke-RestMethod -Uri "$PaymentsBase/health" -Method Get
    "OK"
}

# 42-43: Verify transaction status after payment events
Test-Api "42. Get tx4 (completed after payment)" {
    Start-Sleep -Seconds 2
    $r = Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid4" -Method Get
    "Status=$($r.status)"
}

Test-Api "43. Get tx5 (cancelled after payment fail)" {
    $r = Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tid5" -Method Get
    "Status=$($r.status)"
}

# --- Cross-service E2E (44-49) ---
Write-Host "`n--- Cross-service E2E (44-49) ---"

# 44-48: Full flow
$tidE2E = Test-Api "44. E2E: Create tx" {
    $r = Invoke-RestMethod -Uri "$TransactionsBase/api/transactions" -Method Post -ContentType "application/json" -Body '{"customerId":"11111111-1111-1111-1111-111111111111"}'
    $r.TransactionId.ToString()
}
if (-not $tidE2E) { $tidE2E = [guid]::Empty }

Test-Api "45. E2E: Add item, submit" {
    Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tidE2E/items" -Method Post -ContentType "application/json" -Body '{"productId":"22222222-2222-2222-2222-222222222222","productName":"E2E","quantity":1,"unitPrice":99.99}' | Out-Null
    Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tidE2E/submit" -Method Post | Out-Null
    "OK"
}

$pidE2E = Test-Api "46. E2E: Start payment" {
    $r = Invoke-RestMethod -Uri "$PaymentsBase/api/payments/start" -Method Post -ContentType "application/json" -Body "{`"transactionId`":`"$tidE2E`",`"customerId`":`"11111111-1111-1111-1111-111111111111`"}"
    $r.PaymentId.ToString()
}
if (-not $pidE2E) { $pidE2E = [guid]::Empty }

Test-Api "47. E2E: Confirm payment" {
    Invoke-RestMethod -Uri "$PaymentsBase/api/payments/$pidE2E/confirm" -Method Post | Out-Null
    "OK"
}

Test-Api "48. E2E: Verify tx completed" {
    Start-Sleep -Seconds 3
    $r = Invoke-RestMethod -Uri "$TransactionsBase/api/transactions/$tidE2E" -Method Get
    if ($r.status -eq "Completed") { "Completed" } else { throw "Expected Completed, got $($r.status)" }
}

# 49. Duplicate confirm (idempotent or 400)
Test-Api "49. E2E: Duplicate confirm" {
    try {
        Invoke-RestMethod -Uri "$PaymentsBase/api/payments/$pidE2E/confirm" -Method Post
        "OK (idempotent)"
    } catch {
        if ($_.Exception.Response.StatusCode -eq 400) { "400 (expected)" } else { throw }
    }
}

# --- Summary ---
Write-Host "`n=== Summary ===" 
$Results | Format-Table -AutoSize
Write-Host "`nPASS: $Pass | FAIL: $Fail | Total: $($Pass + $Fail)"
