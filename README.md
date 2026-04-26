#

## Stack

- Frontend: Next.js App Router, Tailwind CSS, Zustand, OpenLayers
- Backend: ASP.NET Core Web API, Entity Framework Core, NetTopologySuite
- Database: PostgreSQL with PostGIS
- Local orchestration: Docker Compose with watch mode

## Requirements

- Docker
- Docker Compose

## Start The Project

From the repository root, run:

```bash
./scripts/dev-up.sh
```

This starts:

- Frontend: [http://localhost:3000](http://localhost:3000)
- Backend API: [http://localhost:8080](http://localhost:8080)
- Scalar API: [http://localhost:8080/scalar](http://localhost:8080/scalar)
- PostgreSQL/PostGIS: localhost:5432

You can also run the same setup directly with:

```bash
docker compose up --watch
```

## Stop The Project

To stop the running stack, press `Ctrl+C` in the terminal that is running Docker Compose.

If you want to stop and remove containers from another terminal, run:

```bash
docker compose down
```

If you also want to remove the PostgreSQL volume and reset the database completely, run:

```bash
docker compose down -v
```

## Useful Commands

Show running services:

```bash
docker compose ps
```

Show logs:

```bash
docker compose logs -f
```

Rebuild services manually:

```bash
docker compose build
```

## Project Structure

```text
.
├── compose.yaml
├── scripts/
├── src/
│   ├── backend/
│   │   └── GeoDemo.*
│   └── frontend/
```

## Notes

- The backend seeds a few spatial features automatically on first startup.
- The recommended development flow is Docker Compose watch mode.

## Quick Validation

Frontend build:

```bash
cd src/frontend
npm run build
```

Backend build:

```bash
cd src/backend
dotnet build GeoDemo.Api/GeoDemo.Api.csproj
```
