# SmartEstimate

SmartEstimate is a SaaS platform for creating professional construction estimates. It combines a structured construction knowledge base, pricing data, and, in later releases, AI-assisted recommendations while keeping every business decision under the user's control.

The first market is Ukraine, initially Kyiv and Kyiv Oblast. The system is designed as a multi-company, multi-country platform.

## Current status

**Sprint 4 — Knowledge Studio MVP is complete.** PostgreSQL is the single source of truth for categories, construction works, materials, and measurement units. The application supports runtime language switching between Ukrainian, English, and German without a page reload.

The current implementation intentionally runs as a temporary single-user/single-company MVP. Authentication, companies, customers, and projects are not implemented yet.

The architecture documents in [`docs/`](docs/) are the source of truth. In case of a conflict, report it before changing the architecture.

## Architecture at a glance

```text
React + TypeScript (Vite) ── REST /api/v1 ──> ASP.NET Core API ──> PostgreSQL
                                                     │
                                                     └──> Python AI services (future)
```

The backend is a modular monolith using Clean Architecture, DDD, and vertical slices. The frontend follows Feature-Sliced Design. The API is the only service allowed to access business data; neither the frontend nor AI services connect directly to PostgreSQL. PostgreSQL is the runtime source of truth for Knowledge; YAML is reserved for future import, export, backup, and catalogue exchange. Localization is a first-class frontend/shared concern backed by resource files and ASP.NET Core request localization infrastructure.

## Repository layout

```text
src/                            .NET solution projects
  SmartEstimate.Api/            HTTP host, Swagger, middleware, DI composition
  SmartEstimate.Application/    application use cases and validation
  SmartEstimate.Domain/         framework-independent domain layer
  SmartEstimate.Infrastructure/ persistence and external integrations
  SmartEstimate.Contracts/      API and integration contracts
  SmartEstimate.Shared/         shared primitives
tests/                          unit and integration test projects
frontend/                       React + TypeScript + Vite application
docker/                         Dockerfiles and nginx configuration
docs/                           approved architecture and product documentation
knowledge/                      future YAML import/export and reference artefacts
python-ai/                      future AI service boundary
```

> Knowledge is implemented as an Application bounded context with repository
> abstractions and EF Core Infrastructure adapters. A separate Knowledge project
> is not part of the approved solution structure.

## Prerequisites

- Docker Desktop (or Docker Engine) with Docker Compose v2
- .NET SDK 9.x for local backend development
- Node.js 20.19+ and npm for local frontend development (the Docker build uses Node 22)

`global.json` defines the .NET SDK baseline. The Docker environment uses .NET 9 and PostgreSQL 17, as required by the architecture documents.

## Start the complete local environment

1. Create local environment settings:

   ```bash
   cp .env.example .env
   ```

2. Replace the example database password in `.env` before sharing or deploying the environment.

3. Build and start all services:

   ```bash
   docker compose up --build
   ```

4. Open the services:

   - Frontend: `http://localhost:3000`
   - Backend Swagger (Development): `http://localhost:8080/swagger`
   - Backend liveness: `http://localhost:8080/health`
   - Backend readiness: `http://localhost:8080/health/ready`

The frontend nginx container proxies `/api/*` to the backend service. PostgreSQL is also exposed locally on port `5432` by default for development tools.

For local Docker development, Compose sets `DATABASE_APPLY_MIGRATIONS_ON_STARTUP=true` and applies the checked-in EF Core migrations at backend startup. Keep this flag disabled in production unless migrations are deliberately run as part of deployment.

Stop services with:

```bash
docker compose down
```

Use `docker compose down -v` only when you intentionally want to remove the local PostgreSQL data volume.

## Local development

Backend:

```bash
dotnet restore SmartEstimate.sln
dotnet build SmartEstimate.sln --no-restore
dotnet run --project src/SmartEstimate.Api/SmartEstimate.Api.csproj
```

Frontend:

```bash
npm install --prefix frontend
npm run dev --prefix frontend
```

The Vite development server runs at `http://localhost:5173` and proxies `/api` to `http://localhost:8080` by default. Its behavior can be adjusted with `VITE_API_PROXY_TARGET` in the local shell environment.

## Smart Estimate Editor

Open `http://localhost:3000/estimates` to create, list, open, and delete estimates. Opening an estimate displays the editor: select a catalog work or material, inspect its automatically supplied unit, enter quantity and unit price, and see totals update. Existing lines can be changed or removed.

Open `/knowledge-studio` to administer Categories, Construction Works, Materials, and Units. Records have Draft, Active, and Archived statuses. Only Active work, material, and unit records are supplied to the Estimate Editor. Ukrainian names are required; English and German names use the approved fallback chain. YAML is not loaded at backend startup.

| Method | Endpoint | Description |
| --- | --- | --- |
| `POST` | `/api/v1/estimates` | Create an estimate, optionally with work and material items. |
| `GET` | `/api/v1/estimates` | Get active estimates (`page`, `pageSize`). |
| `GET` | `/api/v1/estimates/{id}` | Get a complete estimate. |
| `DELETE` | `/api/v1/estimates/{id}` | Soft-delete an estimate. |
| `POST` | `/api/v1/estimates/{id}/work-items` | Add a work selected from the Knowledge Catalog. |
| `PATCH` | `/api/v1/estimates/{id}/work-items/{itemId}` | Change a work line quantity, unit price, or note. |
| `DELETE` | `/api/v1/estimates/{id}/work-items/{itemId}` | Remove a work line. |
| `POST` | `/api/v1/estimates/{id}/material-items` | Add a material selected from the Knowledge Catalog. |
| `PATCH` | `/api/v1/estimates/{id}/material-items/{itemId}` | Change a material line quantity, unit price, or note. |
| `DELETE` | `/api/v1/estimates/{id}/material-items/{itemId}` | Remove a material line. |
| `GET`, `POST`, `PUT`, `DELETE` | `/api/v1/knowledge/categories` | Manage categories; DELETE archives. |
| `GET`, `POST`, `PUT`, `DELETE` | `/api/v1/knowledge/construction-works` | Manage construction works; collections support filters and pagination. |
| `GET`, `POST`, `PUT`, `DELETE` | `/api/v1/knowledge/materials` | Manage materials; DELETE archives. |
| `GET`, `POST`, `PUT`, `DELETE` | `/api/v1/knowledge/units` | Manage measurement units; DELETE archives. |

New estimate lines save the selected PostgreSQL Knowledge identifier together with a name and unit snapshot; later Knowledge edits therefore do not rewrite historical estimates. The user supplies the unit price in this sprint—market pricing is intentionally not implemented. Every line total is rounded to two decimal places; work, material, and grand totals are derived by the `Estimate` aggregate after each add, change, or removal.

Successful reads and creates use the `ApiResponse<T>` envelope. Expected validation, duplicate-number, catalog/not-found errors use the same envelope; unexpected failures use RFC 7807 Problem Details.

## Internationalization

Frontend localization uses `i18next`, `react-i18next`, and `i18next-browser-languagedetector`. Translation resources live under `frontend/src/shared/i18n/locales/{uk,en,de}` and are split into `common.json`, `estimate.json`, `knowledge.json`, `navigation.json`, and `validation.json`. Ukrainian is the default/fallback language.

The language switcher in the application shell changes the UI immediately and stores the selected locale in `localStorage` under `smartestimate.locale`. The shared API client sends `Accept-Language` on every REST call, and shared formatting helpers use `Intl` for dates, numbers, and currency.

Backend localization infrastructure supports `uk-UA`, `en-US`, and `de-DE` through `Accept-Language`. Problem Details responses include the resolved culture, and Swagger title/description can be localized without changing API routes.

## Configuration and secrets

`.env.example` documents the variables used by Docker Compose. Copy it to `.env`; never commit `.env`, credentials, certificates, API keys, or generated artifacts.

The checked-in Compose setup is intended for local development. Production configuration must use a secret manager, HTTPS termination, managed database backups, and environment-specific deployment settings.

PostgreSQL is the runtime source of truth for Knowledge. The `knowledge/` directory is retained for future import, export, backup, and catalogue exchange; the backend does not load YAML during normal operation.

## Useful checks

```bash
docker compose --env-file .env.example config
dotnet build SmartEstimate.sln
npm run lint --prefix frontend
npm run build --prefix frontend
```

For repository conventions and agent guidance, see [`AGENTS.md`](AGENTS.md).
