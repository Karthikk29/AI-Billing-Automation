$baseUrl = "http://localhost:5003/api/billing/process"
$ErrorActionPreference = "Stop"

function Print-ServerResponse {
    param($Exception)
    if ($Exception.Response) {
        $reader = New-Object System.IO.StreamReader($Exception.Response.GetResponseStream())
        $body = $reader.ReadToEnd()
        try {
            # Try to prettify JSON error
            $json = $body | ConvertFrom-Json
            Write-Host "Server Message:"
            $json | ConvertTo-Json -Depth 5
        }
        catch {
            Write-Host "Server Response: $body"
        }
    }
}

Write-Host "Checking input files..."
if (-not (Test-Path "test_payload.json")) { Write-Error "test_payload.json not found."; exit }
if (-not (Test-Path "test_payload_invalid.json")) { Write-Error "test_payload_invalid.json not found."; exit }

$validPayload = Get-Content -Raw "test_payload.json"
$invalidPayload = Get-Content -Raw "test_payload_invalid.json"

# ---------------------------------------------------------
# TEST 1: Valid Attributes
# ---------------------------------------------------------
Write-Host "`n----- Test 1: VALID Payload (Expected: 200 OK) -----" -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri $baseUrl -Method Post -Body $validPayload -ContentType "application/json"
    Write-Host "`n✅ PASSED: Valid payload processed successfully." -ForegroundColor Green
    Write-Host "Response Data:" -ForegroundColor Gray
    $response | ConvertTo-Json -Depth 5
}
catch {
    Write-Host "`n❌ FAILED: Valid payload request failed." -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Print-ServerResponse -Exception $_.Exception
}

# ---------------------------------------------------------
# TEST 2: Invalid Attributes
# ---------------------------------------------------------
Write-Host "`n----- Test 2: INVALID Payload (Expected: 400 Bad Request) -----" -ForegroundColor Cyan
try {
    # We expect this to fail, so if it succeeds, that's actually a failure of validation
    $response = Invoke-RestMethod -Uri $baseUrl -Method Post -Body $invalidPayload -ContentType "application/json"
    
    Write-Host "`n❌ FAILED: Server accepted invalid data (Expected 400 Bad Request)." -ForegroundColor Red
    $response | ConvertTo-Json -Depth 5
}
catch {
    # Check if it was a 400 Bad Request
    if ($_.Exception.Response.StatusCode -eq "BadRequest") {
        Write-Host "`n✅ PASSED: Server correctly rejected invalid data." -ForegroundColor Green
        Write-Host "Validation Errors Received (As Expected):" -ForegroundColor Gray
        Print-ServerResponse -Exception $_.Exception
    }
    else {
        Write-Host "`n❌ FAILED: Unexpected error code." -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        Print-ServerResponse -Exception $_.Exception
    }
}
