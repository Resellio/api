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
   
3. Set up environment variables:

   Create an `appsettings.json` file in the root of the project, following the structure
   of `appsettings.example.json` found in the project. Alternatively, you can use the 
   example below:

   ```json
   {
      "Logging": {
         "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
         }
      },
      "AllowedHosts": "*",
      "AllowedOrigins": [
         "https://example.com",
         "https://another-site.com"
      ]
   }
   ```

4. Run application:

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
