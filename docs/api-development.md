# API Development With Scalar

The API exposes its generated OpenAPI document and a Scalar API reference for local development and trusted-LAN testing. These endpoints are developer/test tooling, not an authentication boundary.

## Quick Path

1. Configure the local API database setting only when testing database-dependent endpoints:

   ```bash
   dotnet user-secrets set "ConnectionStrings:Postgres" "<connection-string>" --project src/DjTracksSessions.Api
   ```

   Keep the real value outside Git and do not put credentials in commands copied into documentation or tickets.
2. Start the API:

   ```bash
   dotnet run --project src/DjTracksSessions.Api
   ```

3. Open [Scalar](http://localhost:5000/scalar) in a browser. Use the actual URL and port printed by `dotnet run` if they differ.

## Endpoints

| URL | Purpose |
|---|---|
| `/openapi/v1.json` | Generated OpenAPI JSON document |
| `/scalar` | Interactive Scalar API reference |

## Test An Operation

1. Open `/scalar` and select an operation from the left navigation.
2. Review the route, parameters, request body, and documented responses.
3. Select **Try it** or **Test Request**, then execute the GET or POST request.
4. Inspect the status code, response headers, and response body shown by Scalar.

The current example endpoints do not require a live database. Database health and future catalog endpoints do. Configure `ConnectionStrings:Postgres` with .NET User Secrets for a locally run API; use only a non-versioned external configuration for containers.

## Safety Boundary

Keep `/openapi/v1.json` and `/scalar` on localhost or the trusted development LAN. Do not expose them to the internet or an untrusted network in a future production topology unless the exposure is explicitly secured with authentication, authorization, and transport protection. Do not use real music-library mounts for automated tests, and do not enter production credentials into Scalar requests.

Docker/NAS manual testing is a separate workflow. When a usable version is selected, run it manually on the NAS with NAS-local configuration and disposable or representative test roots as described in [the NAS workflow](deployment/nas-manual.md).
