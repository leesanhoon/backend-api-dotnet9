# backend-api-dotnet9

Backend API template using ASP.NET Core `.NET 9` for web/mobile clients.

## Features

- Controller-based API (`Category`, `Product`)
- Dependency Injection with service layer (`Controller -> Service -> DbContext`)
- API versioning via URL segment (`/api/v1/...`) and header (`x-api-version`)
- CORS configurable via `appsettings`
- Swagger UI for API exploration
- Health check endpoint (`/health`)
- Problem Details and global exception handler

## Prerequisites

- .NET SDK 9+ (SDK 10 is also compatible for building this project)

## Run locally

```bash
dotnet restore
dotnet run
```

Default local URL is shown in terminal (usually `https://localhost:7xxx`).

## Database (EF Core + PostgreSQL)

This project is configured with EF Core and PostgreSQL provider.

- DbContext: `Data/AppDbContext.cs`
- Services:
  - `Services/Interfaces/ICategoryService.cs`
  - `Services/Interfaces/IProductService.cs`
  - `Services/CategoryService.cs`
  - `Services/ProductService.cs`
- Category CRUD: `/api/v1/Categories`
- Product CRUD: `/api/v1/Products`

Set connection string via environment variable in production:

```bash
ConnectionStrings__DefaultConnection=Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true
```

## Docker deploy

`Dockerfile` is included for container build.

- Render start command for Docker: use default image entrypoint
- Exposed port: `8080`
- Required env vars:
  - `ASPNETCORE_ENVIRONMENT=Production`
  - `ASPNETCORE_URLS=http://0.0.0.0:$PORT`
  - `ConnectionStrings__DefaultConnection=<your-neon-connection-string>`

## Important endpoints

- `GET /` : redirect to Swagger UI
- `GET /health` : health check
- `GET /swagger` : API docs (development)
- `GET /api/v1/Categories`
- `GET /api/v1/Products`

## Configure CORS

Edit:

- `appsettings.Development.json` for local development
- `appsettings.json` for environment defaults

Example:

```json
"Cors": {
  "AllowedOrigins": [
    "https://your-web-client.com",
    "https://your-admin-client.com"
  ]
}
```

If `AllowedOrigins` is empty, API currently allows any origin.

## Suggested next steps

1. Add EF migrations and apply schema to Neon/PostgreSQL.
2. Add authentication (JWT/OAuth2) and authorization policies.
3. Add DTO validation and global error mapping.
4. Add integration tests for critical endpoints.
