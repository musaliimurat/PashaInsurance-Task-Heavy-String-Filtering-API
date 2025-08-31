SOLUTION := PashaInsuranceFiltering.sln
API_DOCKERFILE := src/WebAPI/PashaInsuranceFiltering.WebAPI/Dockerfile

.PHONY: all build test docker-build up down logs

all: build test docker-build up

build:
	dotnet restore $(SOLUTION)
	dotnet build $(SOLUTION) -c Release --no-restore

test:
	dotnet test $(SOLUTION) -c Release --no-build --verbosity normal

docker-build:
	docker compose build

up:
	docker compose up -d
	@echo "waiting for health..."
	@for i in $$(seq 1 20); do \
		curl -sf http://localhost:8080/health >/dev/null && echo "OK" && exit 0; \
		sleep 1; \
	done; \
	echo "health check failed" && exit 1

down:
	docker compose down

logs:
	docker compose logs -f
