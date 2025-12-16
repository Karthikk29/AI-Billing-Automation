$json = Get-Content "appsettings.json" | ConvertFrom-Json
$apiKey = $json.Gemini.ApiKey

if ([string]::IsNullOrWhiteSpace($apiKey)) {
    Write-Error "API Key not found in appsettings.json"
    exit
}

$url = "https://generativelanguage.googleapis.com/v1beta/models?key=$apiKey"

Write-Host "Querying available models..."
try {
    $response = Invoke-RestMethod -Uri $url -Method Get
    $response.models | Format-Table -Property name, version, displayName
}
catch {
    Write-Host "Error listing models: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Server Response: $($reader.ReadToEnd())" -ForegroundColor Red
    }
}
