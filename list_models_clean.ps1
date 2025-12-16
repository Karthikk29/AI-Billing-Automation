$json = Get-Content "appsettings.json" | ConvertFrom-Json
$apiKey = $json.Gemini.ApiKey
$url = "https://generativelanguage.googleapis.com/v1beta/models?key=$apiKey"
try {
    $response = Invoke-RestMethod -Uri $url -Method Get
    $response.models | Select-Object -ExpandProperty name
}
catch {
    Write-Host "Error: $($_.Exception.Message)"
}
