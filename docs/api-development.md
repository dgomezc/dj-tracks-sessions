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

## Debug Both Projects From Visual Studio With WSL

Use this workflow when you want `DjTracksSessions.Api` and `DjTracksSessions.Web` to start together under the Visual Studio debugger:

1. Open `DjTracksSessions.slnx` in Visual Studio Community 2026. Open the solution file, not the repository folder or an individual project.
2. In Solution Explorer, right-click the solution root and select **Configure Startup Projects...**.
3. Select **Multiple startup projects**.
4. Set `DjTracksSessions.Api` and `DjTracksSessions.Web` to **Start**. For each project, select its `WSL` debug target when that target is available.
5. Save the configuration and select the resulting multi-project launch profile in the Visual Studio run/debug target selector.
6. Press **F5**. Visual Studio should launch both processes and open the API and Web URLs defined by their WSL profiles.

The repository's current project profiles define `WSL` for both projects and use the `Ubuntu` distribution. The API uses HTTPS port `2019` (HTTP `2020`); the Web project uses HTTPS port `2021` (HTTP `2022`). Use the URLs printed by Visual Studio if local settings differ.

Visual Studio stores a user-created solution launch configuration in `DjTracksSessions.slnLaunch.user`. It is user-specific and normally is not a machine-independent setup artifact, so configure the profile again on another machine. A shared `DjTracksSessions.slnLaunch` file may be committed only when the team intentionally wants to share that exact launch configuration; it does not replace checking that each machine has the required WSL distribution, SDK, and project prerequisites.

This is direct WSL debugging, not Docker Compose debugging. The WSL profile runs the API and Web projects from the solution under the debugger. Docker Compose builds and runs the containerized services with container configuration, ports, mounts, and an external `ConnectionStrings__Postgres` value; use the Compose workflow when verifying containers or NAS behavior instead.

Before pressing F5, verify that Visual Studio Community 2026 has the .NET/web development tooling, WSL 2 is installed and the `Ubuntu` distribution is available, the .NET 10 SDK is available in WSL, and the repository is accessible from the WSL Linux filesystem. If the projects fail during startup, first confirm the selected debug targets, the WSL distribution, the local User Secrets/database configuration required by database-dependent endpoints, and the HTTPS URLs shown by Visual Studio. Do not use the production music-library mounts for this local debugging loop.

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
