Write-Host "Starting FlowBoard Microservices..." -ForegroundColor Green

# Array of all microservices
$services = @(
    "FlowBoard.Auth",
    "FlowBoard.Workspace",
    "FlowBoard.Board",
    "FlowBoard.Column",
    "FlowBoard.Task",
    "FlowBoard.Comment",
    "FlowBoard.Checklist",
    "FlowBoard.Notification"
)

# Start each microservice in the background
foreach ($service in $services) {
    Write-Host "Starting $service..." -ForegroundColor Cyan
    Start-Process -NoNewWindow dotnet -ArgumentList "run --project $service"
}

# Finally, start the Gateway explicitly on port 5050
Write-Host "Starting FlowBoard.Gateway on Port 5050..." -ForegroundColor Yellow
Start-Process -NoNewWindow dotnet -ArgumentList "run --project FlowBoard.Gateway --urls http://localhost:5050"

Write-Host "All services started successfully! You can now use the frontend." -ForegroundColor Green
