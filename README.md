# ApiPerfDashboard

A simple API performance dashboard built with **.NET 8 + Postgres + Blazor Server**.

## Features
- Collector API (Minimal API)
  - `POST /metrics` → insert metric
  - `GET /metrics/summary` → returns average latency, error % by endpoint
  - Table auto-creates on first use
  - Swagger UI enabled

- Dashboard (Blazor Server)
  - Filters: time range (1h, 24h, 7d)
  - Shows average latency, error % by endpoint

- Postgres for storage
- Dockerized setup (`docker-compose.yml`)

## Run

```bash
docker compose up --build
```

- API: http://localhost:5001/swagger
- Dashboard: http://localhost:5002
- Postgres: localhost:5432 (db: metrics, user: metrics, password: metrics)
