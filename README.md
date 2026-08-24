# GameStore

A REST API for an online game store: games, genres, platforms, publishers, orders, users, roles and comments, with a standalone payment microservice and an external auth service, backed by SQL Server and MongoDB.

## Architecture

| Component | Description | Tech |
|---|---|---|
| `src/Gamestore.WebApi` | Main REST API (games, genres, platforms, publishers, orders, users, roles, comments) | ASP.NET Core |
| `src/Gamestore.BLL` | Business logic, services, validators, mapping | .NET |
| `src/Gamestore.DAL` | Data access, EF Core migrations, Mongo repositories | .NET, EF Core, MongoDB.Driver |
| `src/Gamestore.Domain` | Entities, enums, exceptions, shared types | .NET |
| `microservice` | Payment processing microservice (Bank/Visa/IBox) | ASP.NET Core |
| `AuthService` | External authentication/authorization API | ASP.NET Core |
| `mongo-init-scripts` | Seeds the Northwind dataset into MongoDB on container start | Mongo shell scripts |
| `tests` | Unit tests for the BLL layer | xUnit |
| `src/Gamestore.UI` / `gamestore-ui-app` | Angular client for the API | Angular |

Data stores: SQL Server holds the primary Gamestore domain data (via EF Core migrations), MongoDB holds the Northwind reference dataset.

## Running locally

The stack is defined in [docker-compose.yml](docker-compose.yml):

```bash
docker compose up --build
```

This starts:

| Service | URL |
|---|---|
| Web API | http://localhost:8080 |
| Client (Angular) | http://localhost:4200 |
| Payment microservice | http://localhost:5000 |
| SQL Server | localhost:1433 |
| MongoDB | localhost:27017 |

The `AuthService` is not containerized — run it separately (it's expected at `http://localhost:5037` via `host.docker.internal`).

## Tests

```bash
dotnet test
```
