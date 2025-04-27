# Resellio API

Resellio API is the backend service for the Resellio application, providing an interface for client applications.

## Getting started

### Prerequisites

- dotnet version >= `9.0`
- docker version >= `28.0`

### External Services

This project uses the following external services:

- **PostgreSQL** – as the main relational database.
- **Redis** – for caching and other fast in-memory operations.

Both services are managed using **Docker Compose**, and are defined in the `docker-compose.yml` file. When running locally, Docker will automatically provision and run these services.

### Running locally

1. Clone the repository:

   ```bash
   git clone https://github.com/Resellio/api.git
   cd api
   ```

2. Setup external services using docker:

   ```bash
   docker compose up
   ```

3. Set up environment variables:

   Create an `appsettings.json` file in the root of the project, following the structure
   of `appsettings.example.json` found in `TickAPI/TickAPI/appsettings.example.json`.

4. Install Entity Framework and set up the database
   
   ```bash
   dotnet tool install --global dotnet-ef --version 9.*
   dotnet ef database update
   ```

6. Run application:

   ```bash
   cd TickAPI/TickAPI
   dotnet run
   ```

   You can optionally specify optimized configuration:

   ```bash
   dotnet run --configuration Release
   ```

### Testing

1. Test application using `dotnet` cli:
   ```bash
   cd TickAPI/TickAPI.Tests
   dotnet test
   ```
