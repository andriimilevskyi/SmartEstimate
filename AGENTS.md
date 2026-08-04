# SmartEstimate contributor guide

## Source of truth

Read the relevant files in `docs/` before making architectural or implementation decisions. The approved documents define Clean Architecture, DDD, vertical slices in the application layer, Feature-Sliced Design in the frontend, PostgreSQL 17, and .NET 9.

If documentation conflicts with the repository or with another document, report the conflict. Do not silently change the architecture to resolve it.

## Boundaries

- Keep domain code framework-independent. `SmartEstimate.Domain` must not depend on EF Core, ASP.NET Core, or infrastructure projects.
- Dependencies point inward: API and Infrastructure may depend on Application/Domain; Application may depend on Domain/Contracts/Shared; Domain does not depend on outer layers.
- Keep HTTP concerns in `SmartEstimate.Api`; controllers or endpoint handlers must not contain business workflows.
- Organize application behavior by vertical feature slice. Do not introduce business functionality outside an approved sprint scope.
- The API is the sole path from the frontend and Python AI services to business data. Do not add direct database access outside Infrastructure.
- Version public HTTP endpoints under `/api/v1`. Keep request validation out of controllers.
- Keep construction knowledge independent from AI prompts and provider-specific integrations.

## Repository conventions

- `src/` holds .NET projects; `tests/` holds tests; `frontend/` holds the React app; `docker/` holds container assets.
- Keep reusable construction-knowledge YAML in `knowledge/`; validate references before adding or changing records.
- Use `Guid`/UUID identifiers and asynchronous APIs with `CancellationToken` where an operation can block.
- Do not commit secrets, `.env`, certificates, build output, dependency directories, coverage output, or local IDE settings.
- Do not edit generated files manually. Keep lock files committed when a package manager creates them.

## Quality gates

Run the narrowest relevant checks before handing off a change:

```bash
dotnet build SmartEstimate.sln
npm run lint --prefix frontend
npm run build --prefix frontend
docker compose --env-file .env.example config
```

For a full local smoke test:

```bash
cp .env.example .env
docker compose up --build
```

Then verify `http://localhost:8080/health`, `http://localhost:8080/health/ready`, `http://localhost:8080/swagger`, and `http://localhost:3000`.

## Docker conventions

- Compose service names are `postgres`, `backend`, and `frontend`.
- Backend listens on container port `8080`; frontend nginx listens on port `80`.
- The frontend accesses the backend through the same-origin `/api` proxy in Compose.
- Use environment variables for deployment-specific settings. The example values are for local development only.
