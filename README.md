# PashaInsuranceFiltering

A **.NET 9 Web API** project that demonstrates **chunked upload + background text filtering** (banned words/phrases), designed with **CQRS**, **DDD-inspired** principles, and **Clean/Onion Architecture**.  
Large text is uploaded in chunks, assembled in memory, filtered asynchronously in a background worker, and stored in-memory for retrieval.

---

## 🏗 Architecture & Design Principles

This project follows modern enterprise-grade architecture practices:

- **Clean Architecture + Onion Architecture**  
  Core Domain at the center, surrounded by Application, Infrastructure, and UI. Dependencies only point inwards.
- **DDD (Domain-Driven Design)**  
  Entities, AggregateRoots, ValueObjects, DomainEvents, and DomainExceptions. Business rules live in the domain.
- **CQRS**  
  Separation of Commands (write) and Queries (read). Handlers wired via MediatR.
- **SOLID Principles**  
  Especially **D (Dependency Inversion)**: Application depends on abstractions (Ports), Infrastructure implements them.
- **Cross-cutting concerns**  
  Validation (FluentValidation) handled via MediatR `PipelineBehavior`. Result/Errors are reusable types stored in SharedKernel.
- **Background Processing**  
  `FilteringWorker` runs as a BackgroundService: dequeues full text, applies filtering, stores result.
- **In-memory adapters**  
  For demo/testing: `InMemoryUploadBuffer`, `InMemoryProcessingQueue`, `InMemoryResultStore`, `InMemoryTextFilter`. In real-world scenarios, replace with DB/Queue providers.

---

## Why SharedKernel?

### Why split into `SharedKernel.Domain` and `SharedKernel.Application`?

This separation enforces **purity** and prevents **dependency cycles**:

- **SharedKernel.Domain**
  - Contains **pure domain primitives** and utilities:
    - `ValueObject` base class
    - Domain exceptions (`DomainValidationException`, `NotFoundException`, etc.)
    - Guard utilities (null/empty checks, etc.)
  - **No dependencies** on external packages (MediatR, FluentValidation, etc.)
  - Purpose: Domain and other modules can safely reference it without introducing higher-level concerns.

- **SharedKernel.Application**
  - Contains **application-level cross-cutting types**:
    - `Result`, `DataResult<T>`, `SuccessResult`, `ErrorResult`, etc.
    - MediatR **pipeline behaviors** (e.g. `ValidationBehavior<,>`)
    - Application service abstractions (e.g. `IClock`) if needed
  - **May depend** on application packages (MediatR, FluentValidation).
  - Purpose: Multiple modules can reuse application concerns, future-proof for modular monolith or microservices.

**Result:**  
Domain remains pure, Application concerns are centralized, Infrastructure/UI stay on the outside. This makes the solution **scalable and maintainable**.

---

## 📂 Project Structure

```
src/
 ├─ PashaInsuranceFiltering.Domain/               # Domain Layer
 │   ├─ Primitives (Entity, AggregateRoot, ValueObject)
 │   ├─ Events (e.g., TextFilteredDomainEvent)
 │   ├─ Exceptions (DomainValidationException, etc.)
 │   └─ Entities (TextDocument, FilteredText, SimilarityThreshold)
 │
 ├─ PashaInsuranceFiltering.Application/          # Application Layer
 │   ├─ Features/Upload (Commands, Validators, Handlers)
 │   ├─ Features/Result (Queries, Handlers, DTOs)
 │   └─ Common/Ports (IUploadBuffer, IProcessingQueue, IResultStore, ITextFilter, ISimilarityMetric)
 │
 ├─ PashaInsuranceFiltering.Infrastructure/       # Infrastructure Layer
 │   ├─ Persistence/InMemory (InMemoryUploadBuffer, InMemoryResultStore)
 │   ├─ Messaging (InMemoryProcessingQueue)
 │   ├─ Filtering (InMemoryTextFilter, JaroWinklerMetric, LevenshteinMetric)
 │   └─ Background (FilteringWorker)
 │
 ├─ PashaInsuranceFiltering.SharedKernel.Domain/  # Cross-cutting: pure domain utilities
 ├─ PashaInsuranceFiltering.SharedKernel.Application/ # Cross-cutting: Result, ValidationBehavior<,>
 ├─ PashaInsuranceFiltering.DependencyInjection/  # Autofac module registrations
 └─ PashaInsuranceFiltering.WebAPI/               # API Layer (Controllers, Program.cs)

tests/
 └─ PashaInsuranceFiltering.Tests.Unit/           # xUnit + FluentAssertions unit tests
```

---

## 🧰 Technologies

- **.NET 9.0** – latest LTS runtime
- **MediatR** – CQRS pipeline (Command/Query separation)
- **FluentValidation** – declarative validation for requests
- **FluentAssertions** – expressive test assertions
- **Autofac** – modular DI container
- **xUnit** – test framework
- **Docker + Compose** – containerized cross-platform build/run
- **Make / PowerShell / Bash scripts** – unified build/test/run across Linux, macOS, Windows

---

## 📥 Getting Started

Clone the repo:

```bash
git clone https://github.com/username/PashaInsuranceFiltering.git
cd PashaInsuranceFiltering
```

---

## ▶️ Running the Project

We provide **cross-platform scripts** to restore, build, run tests, build Docker image, and run the API. run root folder =>

### Linux/macOS – Makefile
```bash
make all           # build + test + docker build + up + health check
make build
make test
make docker-build
make up
make logs
make down
```

### macOS – Bash script
```bash
chmod +x scripts/run-all-macos.sh
./scripts/run-all-macos.sh
```

### Windows – PowerShell
```powershell
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned   # one-time
terminal run command =>  .\scripts\run-all.ps1
```

**Each script does:**
- `dotnet restore/build/test` on solution
- `docker compose build`
- `docker compose up -d`
- Waits for `http://localhost:8080/health` (service health check)

---

## Testing

Manual test run:
```bash
dotnet test PashaInsuranceFiltering.sln -c Release
```

Unit tests cover:
- `InMemoryTextFilter` → token/phrase filtering
- `JaroWinklerMetric` and `LevenshteinMetric`
- `InMemoryUploadBuffer` → chunking, ordering, max size limit
- `InMemoryProcessingQueue` → FIFO behavior
- `FilteringWorker` → background end-to-end flow

---

## 🌐 Docker Usage

### Dockerfile & Compose
- **Dockerfile**: `PashaInsuranceFiltering.WebAPI/Dockerfile`
- **docker-compose.yml**: root directory

Compose configuration:
```yaml
services:
  filtering-api:
    build:
      context: .
      dockerfile: PashaInsuranceFiltering.WebAPI/Dockerfile
    ports:
      - "8080:8080"
    environment:
      ASPNETCORE_URLS: "http://+:8080"
      ASPNETCORE_ENVIRONMENT: "Development"
      Filtering__Strategy: "JaroWinkler"        # or "Levenshtein"
      Filtering__Threshold: "0.85"              # between 0..1
      Filtering__BannedWords__0: "secret"
      Filtering__BannedWords__1: "password"
      Filtering__BannedWords__2: "api key"
      Filtering__BannedWords__3: "confidential"
      Filtering__BannedWords__4: "credit cart"
      Filtering__BannedWords__5: "cvv"
      Filtering__BannedWords__6: "card number"
```

Run:
```bash
docker compose build
docker compose up -d
```

Access:
- Health check: [http://localhost:8080/health](http://localhost:8080/health)
- Swagger: [http://localhost:8080/swagger/index.html](http://localhost:8080/swagger/index.html)

---

## API Examples

### Upload chunk (Linux/macOS)
```bash
curl -X POST http://localhost:8080/api/upload  -H "Content-Type: application/json"  -d '{"uploadId":"11111111-1111-1111-1111-111111111111","chunkIndex":0,"data":"This is my secret password and API key.","isLastChunk":false}'
```
```bash
curl -X POST http://localhost:8080/api/upload  -H "Content-Type: application/json"  -d '{"uploadId":"11111111-1111-1111-1111-111111111111","chunkIndex":1,"data":"THe gave me the credit card details, such as the password and CVV code.","isLastChunk":true}'
```

### Upload chunk (Windows PowerShell)
```powershell
Invoke-RestMethod http://localhost:8080/api/upload `
 -Method POST `
 -ContentType "application/json" `
 -Body '{"uploadId":"11111111-1111-1111-1111-111111111111","chunkIndex":0,"data":"This is my secret password and API key.","isLastChunk":false}'
```
```powershell
Invoke-RestMethod http://localhost:8080/api/upload `
 -Method POST `
 -ContentType "application/json" `
 -Body '{"uploadId":"11111111-1111-1111-1111-111111111111","chunkIndex":1,"data":"He gave me the credit card details, such as the password and CVV code.","isLastChunk":true}'
```

### Get result
```bash
curl http://localhost:8080/api/result/11111111-1111-1111-1111-111111111111
```

PowerShell:
```powershell
Invoke-RestMethod http://localhost:8080/api/result/11111111-1111-1111-1111-111111111111 -Method GET
```

Response examples:
```json
// Completed
{
  "uploadId": "11111111-1111-1111-1111-111111111111",
  "status": "Completed",
  "data": "This is my  and . THe gave me the  details, such as the  and  code."
}

// Pending
{
  "uploadId": "11111111-1111-1111-1111-111111111111",
  "status": "Pending",
  "data": null
}

// NotFound
{
  "uploadId": "11111111-1111-1111-1111-111111111111",
  "status": "NotFound",
  "data": null
}
```

---

##  Running with Visual Studio

1. Open `PashaInsuranceFiltering.sln`  
2. Set **PashaInsuranceFiltering.WebAPI** as startup project  
3. Press `Ctrl+F5` or `F5`  
4. Swagger opens at:  
   `http://localhost:<VS-port>/swagger/index.html`

---

## ⚙️ Configuration

- **Filtering.Strategy**: `JaroWinkler` | `Levenshtein`
- **Filtering.Threshold**: double (0..1)
- **Filtering.BannedWords**: array of strings

These are provided via **appsettings.json** or environment variables.

---

## Why This Architecture?

- **Domain** – core business rules, pure and isolated.
- **Application** – orchestrates use-cases, defines ports.
- **Infrastructure** – implements ports (storage, queue, filters).
- **SharedKernel.Domain** – reusable domain utilities (pure).
- **SharedKernel.Application** – cross-application concerns (Result, Behaviors).
- **DependencyInjection** – centralizes DI container configuration.
- **WebAPI** – entrypoint, controllers, swagger, health.

This structure makes the system **extensible**, easy to test, and ready for **future modular monolith or microservice migration**.

---

## 🛠 Troubleshooting

- **Docker engine error (`//./pipe/dockerDesktopLinuxEngine`)**  
  → Docker Desktop must be running. Verify with `docker version`.

- **PowerShell curl flags not working**  
  → Use `Invoke-RestMethod` instead (examples provided above).

- **Port conflicts**  
  → Change port mapping in `docker-compose.yml`: e.g., `"8081:8080"`.

---

## Summary

This repository demonstrates a **Filtering API** built with **Clean Architecture + DDD principles**, **CQRS**, fully **unit-tested**, and easily **runnable via Docker** across **Linux, macOS, and Windows**.  
The architecture is designed for **long-term scalability** and **clean separation of concerns**.
