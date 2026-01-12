# Setup firewall rules for SGRRHH
# Run as Administrator

$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: Requires Administrator privileges" -ForegroundColor Red
    exit 1
}

Write-Host "Creating firewall rules for SGRRHH..." -ForegroundColor Cyan

# Remove existing rules if any
Get-NetFirewallRule -DisplayName "SGRRHH*" -ErrorAction SilentlyContinue | Remove-NetFirewallRule

# Create HTTP rule (port 5002)
New-NetFirewallRule -DisplayName "SGRRHH - HTTP 5002" -Direction Inbound -Protocol TCP -LocalPort 5002 -Action Allow -Profile Private,Domain -Enabled True | Out-Null
Write-Host "  OK: HTTP port 5002 opened" -ForegroundColor Green

# Create HTTPS rule (port 5003)
New-NetFirewallRule -DisplayName "SGRRHH - HTTPS 5003" -Direction Inbound -Protocol TCP -LocalPort 5003 -Action Allow -Profile Private,Domain -Enabled True | Out-Null
Write-Host "  OK: HTTPS port 5003 opened" -ForegroundColor Green

Write-Host ""
Write-Host "Firewall configured successfully!" -ForegroundColor Green
Write-Host "Access URLs from other PCs:" -ForegroundColor Yellow
Write-Host "  http://192.168.1.248:5002" -ForegroundColor White
Write-Host "  https://192.168.1.248:5003" -ForegroundColor White
