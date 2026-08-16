---
name: dotnet-vertical-slice
description: "Trigger: endpoint, vertical slice, FluentResults, FluentValidation. Implement one ASP.NET Core slice with explicit contracts and tests."
license: Apache-2.0
metadata:
  author: dj-tracks-sessions
  version: "1.0"
---

# .NET Vertical Slice

## Activation Contract

Use when adding or changing an ASP.NET Core API command/query, validation rule, or Blazor-to-API contract.

## Hard Rules

- Keep request, response, endpoint, validator, handler, mapping, and tests owned by the feature slice.
- Keep API contracts independent from EF entities and Blazor view models.
- Validate explicitly with `ValidateAsync`; never use synchronous MVC auto-validation.
- Return expected failures with `FluentResults` and stable error codes.
- Map failures to the shared Problem Details contract; do not create endpoint-specific error envelopes.
- Do not introduce MediatR, repositories, or global service layers without an evidenced need.
- Persist through Entity Framework Core with Npgsql; do not use EF InMemory or SQLite as a PostgreSQL substitute.
- Add reviewed EF Core migrations for schema changes and exercise them against a disposable PostgreSQL container.

## Decision Gates

| Behavior | Test boundary |
|---|---|
| Pure invariant or mapping | Unit test |
| Endpoint, validation, persistence | API integration test with PostgreSQL Testcontainer |
| Filesystem/provider/audio adapter | Integration or contract test with disposable fixtures |

## Execution Steps

1. Load current ASP.NET Core, FluentValidation, FluentResults, EF Core, and Npgsql documentation through Context7 when API details matter.
2. Define the smallest request/response contract and validation rules.
3. Implement the handler with explicit cancellation and result failures.
4. Expose the endpoint and map outcomes consistently.
5. Add behavior-first tests including one failure path; use real PostgreSQL for translated queries, constraints, and transactions.
6. Verify dependency direction and OpenAPI output when contracts change.

## Output Contract

Return the slice path, public contract, error codes, tests run, and any intentional infrastructure dependency.

## References

- `../../../AGENTS.md`
- `../../../PLAN.md`
