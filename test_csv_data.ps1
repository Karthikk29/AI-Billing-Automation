$csvPath = "CustomerData - Dev Task (1) - Plans.csv"
$baseUrl = "http://localhost:5003/api/billing/process"
$outputPath = "billing_response_full.json"

if (-not (Test-Path $csvPath)) {
    Write-Error "CSV File not found: $csvPath"
    exit
}

Write-Host "Reading CSV data..." -ForegroundColor Cyan
$csvData = Import-Csv $csvPath

Write-Host "Converting to JSON..." -ForegroundColor Cyan
$payload = $csvData | ForEach-Object {
    [PSCustomObject]@{
        customerId = $_.CustomerID
        planName   = $_.Plan
        cityRegion = $_.Region
        usage      = [int]($_.Usage -replace ',', '')
    }
}

$jsonPayload = $payload | ConvertTo-Json -Depth 5 -Compress

Write-Host "Sending $($payload.Count) records to Billing API..." -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri $baseUrl -Method Post -Body $jsonPayload -ContentType "application/json"
    
    Write-Host "✅ Success! API processed the data." -ForegroundColor Green
    
    # Save formatted JSON to file
    $response | ConvertTo-Json -Depth 10 | Set-Content $outputPath
    Write-Host "Detailed response saved to '$outputPath'" -ForegroundColor Gray

    # Show summary to console
    Write-Host "`n--- Summary ---" -ForegroundColor Yellow
    Write-Host "Processed Count: $($response.processedCustomers.Count)"
    Write-Host "Rejected Count:  $($response.rejectedCustomers.Count)"
    
    if ($response.aiSummary) {
        Write-Host "`n--- AI Analysis ---" -ForegroundColor Yellow
        # Handle if AI summary is an object or string
        if ($response.aiSummary.summary) {
            Write-Host $response.aiSummary.summary
        }
        else {
            $response.aiSummary | ConvertTo-Json -Depth 2
        }
    }

}
catch {
    Write-Host "❌ Request Failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Server Response: $($reader.ReadToEnd())" -ForegroundColor Red
    }
}
