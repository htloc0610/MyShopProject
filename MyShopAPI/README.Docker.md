# MyShopAPI - Docker Setup Guide

This guide explains how to build and run the MyShopAPI using Docker and Docker Compose.

## 📋 Prerequisites

- Docker Desktop installed (Windows/Mac/Linux)
- Docker Compose (included with Docker Desktop)

## 🏗️ Project Structure

```
MyShopAPI/
├── Dockerfile                 # Multi-stage build for the API
├── docker-compose.yml         # Development environment
├── docker-compose.prod.yml    # Production environment
├── .dockerignore              # Files to exclude from Docker build
├── .env                       # Environment variables
└── README.Docker.md           # This file
```

## 🚀 Quick Start

### Development Environment

1. **Start all services** (API + PostgreSQL + PgAdmin):
   ```bash
   docker-compose up -d
   ```

2. **View logs**:
   ```bash
   docker-compose logs -f api
   ```

3. **Stop all services**:
   ```bash
   docker-compose down
   ```

### Production Environment

1. **Start production services** (API + PostgreSQL, no PgAdmin):
   ```bash
   docker-compose -f docker-compose.prod.yml up -d
   ```

## 🔧 Configuration

### Environment Variables (.env)

The `.env` file contains all configuration:

- **Database**: PostgreSQL credentials and connection settings
- **JWT**: Secret keys and token expiry settings
- **PgAdmin**: Admin interface credentials

**⚠️ IMPORTANT**: Change the default passwords in production!

### Connection Strings

The API connects to PostgreSQL using environment variables:
- **Development**: `Host=postgres;Port=5432;Database=myshop;Username=admin;Password=secret`
- **Production**: Use strong passwords from `.env` file

## 📦 Services

### 1. PostgreSQL Database
- **Port**: 5432
- **Container**: `myshop-postgres`
- **Volume**: `postgres_data` (persists database)
- **Health Check**: Ensures database is ready before API starts

### 2. PgAdmin (Development Only)
-   **Port**: 5050
-   **Container**: `myshop-pgadmin`
-   **URL**: http://localhost:5050
-   **Login**: admin@local.com / admin

### 3. MyShop API
-   **Port**: 5002
-   **Container**: `myshop-api`
-   **URL**: http://localhost:5002
-   **Swagger**: http://localhost:5002/swagger
-   **Volume**: `api_wwwroot` (persists uploaded images)

## 🛠️ Common Commands

### Build and Run

```bash
# Build images
docker-compose build

# Build without cache
docker-compose build --no-cache

# Start services
docker-compose up -d

# Start and rebuild
docker-compose up -d --build
```

### Manage Services

```bash
# View running containers
docker-compose ps

# View logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f api

# Restart a service
docker-compose restart api

# Stop all services
docker-compose down

# Stop and remove volumes (⚠️ deletes data)
docker-compose down -v
```

### Database Management

```bash
# Access PostgreSQL CLI
docker exec -it myshop-postgres psql -U admin -d myshop

# Backup database
docker exec myshop-postgres pg_dump -U admin myshop > backup.sql

# Restore database
docker exec -i myshop-postgres psql -U admin myshop < backup.sql
```

### API Container Access

```bash
# Access API container shell
docker exec -it myshop-api bash

# View API logs
docker logs -f myshop-api
```

## 🔍 Troubleshooting

### API won't start
1.  Check if PostgreSQL is healthy:
    ```bash
    docker-compose ps
    ```
2.  View API logs:
    ```bash
    docker-compose logs api
    ```

### Database connection failed
1.  Ensure PostgreSQL is running:
    ```bash
    docker-compose ps postgres
    ```
2.  Check connection string in `.env`
3.  Verify network connectivity:
    ```bash
    docker network inspect myshopapi_myshop-network
    ```

### Port already in use
Change the port mapping in `docker-compose.yml`:
```yaml
ports:
  - "5003:5002"  # Change outer port to 5003
```

## 🔐 Security Notes

### For Production:

1. **Change default passwords** in `.env`:
   - `POSTGRES_PASSWORD`
   - `JWT_SECRET`
   - `PGADMIN_DEFAULT_PASSWORD`

2. **Use secrets management**:
   - Consider Docker Secrets or environment-specific `.env` files
   - Never commit `.env` to version control

3. **Enable HTTPS**:
   - Use a reverse proxy (nginx, Traefik)
   - Configure SSL certificates

4. **Restrict network access**:
   - Don't expose PostgreSQL port publicly
   - Use firewall rules

## 📊 Monitoring

### Health Checks

Both services have health checks:
- **PostgreSQL**: `pg_isready` command
- **API**: HTTP request to `/weatherforecast`

Check health status:
```bash
docker-compose ps
```

## 🔄 Database Migrations

The API automatically runs migrations on startup. To run manually:

```bash
# Access API container
docker exec -it myshop-api bash

# Run migrations (if needed)
dotnet ef database update
```

## 📝 Notes

- **Data Persistence**: All data is stored in Docker volumes and persists across container restarts
- **Development Mode**: Database seeding runs automatically in Development environment
- **Static Files**: Product images are stored in the `api_wwwroot` volume
- **Network**: All services communicate via the `myshop-network` bridge network

## 🆘 Support

For issues or questions:
1. Check logs: `docker-compose logs -f`
2. Verify configuration in `.env`
3. Ensure Docker Desktop is running
4. Check port availability
