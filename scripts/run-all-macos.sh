#!/usr/bin/env bash
set -euo pipefail

SOLUTION="PashaInsuranceFiltering.sln"

echo "===> restore & build"
dotnet restore "$SOLUTION"
dotnet build "$SOLUTION" -c Release --no-restore

echo "===> unit tests"
dotnet test "$SOLUTION" -c Release --no-build --verbosity normal


if docker info >/dev/null 2>&1; then
  echo "===> docker compose build"
  docker compose build

  echo "===> docker compose up"
  docker compose up -d

  echo "===> health check"
  for i in {1..30}; do
    if curl -sf http://localhost:8080/health >/dev/null; then
      echo "service healthy"
      exit 0
    fi
    sleep 1
  done

  echo "health check failed"
  exit 1
else
  echo " Docker Desktop don't run."
  echo "   only run build + unit test"
fi
