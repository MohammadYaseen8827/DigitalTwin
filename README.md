# Unified Digital Twin Platform (Predictive + Modular Merge)

Authoritative source: `MASTER_INTEGRATION_STRATEGY.md`. This scaffold aligns with the merged architecture (backend .NET 7, frontend Vue 4/TypeScript, PostgreSQL, Docker).

## Structure
- `src/api/DigitalTwinPlatform.API`: .NET 7 Web API (controllers, services, repositories, ML, simulation, SignalR).
- `src/ui/digital-twin-dashboard`: Vue + TypeScript dashboard (Pinia, Vue Router, ECharts/Three.js ready).
- `infrastructure`: Dockerfiles, docker-compose, CI/CD.
- `docs`: Secondary docs (to be expanded).

## Quickstart
1) Prereqs: .NET 7 SDK, Node 18+, Docker Desktop, PostgreSQL client.
2) Copy env template: `cp .env.example .env` and adjust secrets.
3) Run all services: `docker-compose up --build`.
4) Backend only (dev): `dotnet run --project src/api/DigitalTwinPlatform.API/DigitalTwinPlatform.API.csproj`.
5) Frontend only (dev): `cd src/ui/digital-twin-dashboard && npm install && npm run dev`.

## Integration Flow (per strategy)
Simulation → Telemetry Service (JSONB) → Predictive Analytics (ML.NET) → Twin Engine update → SignalR broadcast → Vue dashboard.

## TODO map (by phases/sprints)
- Phase 1/Sprint 1: Unify DB context, GUID IDs, compose setup. (See `Data/DigitalTwinDbContext.cs`, `docker-compose.yml`).
- Phase 2/Sprint 2: Repository + service interfaces fully wired; add AutoMapper/FluentValidation. (See `Services/*`, `Repositories/*`).
- Phase 3-4/Sprint 3-4: ML.NET pipelines + degradation models; integrate into simulation loop. (See `Services/Analytics/ML/*`, `Services/Simulation/*`).
- Phase 5/Sprint 5: Vue dashboard merge, Pinia stores, ECharts/Three.js hooks. (See `src/ui/...`).
- Phase 6/Sprint 6: Tenant schemas, context switching. (See `Services/Infrastructure/TenantService.cs`).
- Phase 7: Tests (unit/integration/E2E). (`DigitalTwinPlatform.Tests` placeholder).
- Phase 8: CI/CD + prod hardening.

## Notes
- Multi-tenancy: schema-per-tenant; connection is resolved per request (see TenantService stub).
- ML.NET: placeholder predictors/trainers with TODOs; wire real models per roadmap.
- SignalR: `TelemetryHub` stub exposes channel for real-time telemetry/predictions.
- Azure Digital Twins: hooks in `Integration/AzureDigitalTwinService.cs` + DTDL samples under `AzureIntegration/`.

## Next Steps
- Fill appsettings with connection strings and JWT/signing keys.
- Add EF Core migrations and run against PostgreSQL.
- Implement real business logic in services and generators.
- Expand dashboard visuals and bind to live API.
