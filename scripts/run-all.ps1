$ErrorActionPreference = "Stop"
$solution = "PashaInsuranceFiltering.sln"

Write-Host "===> restore & build"
dotnet restore $solution
dotnet build $solution -c Release --no-restore

Write-Host "===> unit tests"
dotnet test $solution -c Release --no-build --verbosity normal

function Test-Docker {
  try { docker version | Out-Null; return $true } catch { return $false }
}

if (Test-Docker) {
  Write-Host "===> docker compose build"
  docker compose build

  Write-Host "===> docker compose up"
  docker compose up -d

  Write-Host "===> health check"
  $ok = $false
  for ($i = 0; $i -lt 20; $i++) {
    try { Invoke-WebRequest http://localhost:8080/health -UseBasicParsing | Out-Null; $ok = $true; break }
    catch { Start-Sleep -Seconds 1 }
  }
  if ($ok) { Write-Host "service healthy" } else { throw "health check failed" }
}
else {
  Write-Host "Docker Desktop don't run."
  Write-Host " Only run build + unit test."
}
