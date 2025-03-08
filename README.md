# Resellio API

Resellio API is the backend service for the Resellio application, providing an interface for client applications.

## Getting started

### Prerequisites

- dotnet version >= `9.0`
- docker version >= `28.0`

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

3. Run application:

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
