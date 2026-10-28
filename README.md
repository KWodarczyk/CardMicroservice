# CardMicroserviceApi Solution

This solution demonstrates a small card-actions rule engine exposed as a microservice,
split across two ASP.NET Core Web APIs to show a realistic service-to-service setup
with an auto-generated OpenAPI client.

## Projects

### CardMicroserviceApi (the façade / main service)

The main entry point for consumers. It exposes `CardActionsController`, which:

- Validates the incoming `userId`/`cardNumber` request (presence, length, allowed
  characters - see `Validation/CardRequestValidator.cs`).
- Looks up card details through `ICardService` (see below).
- Runs the card details through `CardActionsCalculator`, a declarative rule engine
  (`ActionRule` + `PinCondition`) that decides which `CardAction`s are allowed for a
  given card type, card status and PIN-set state.
- Also exposes a `POST /api/CardActions/evaluate` endpoint that runs the rule engine
  directly against an explicit card type/status/PIN combination, without needing a
  real card lookup - handy for exploring/testing the rules via Swagger.

`ICardService` has two implementations, selected at startup via the
`CardDataApi:UseMock` setting in `appsettings.json`:

- **`CardServiceMock`** - an in-memory, hard-coded set of sample cards. No network
  calls, no second project required. This is enough to run and explore the whole
  rule engine on its own.
- **`CardServiceClient`** - calls the `CardDataApi` backend microservice over HTTP
  using an auto-generated client (see "OpenAPI client generation" below).

Both implementations return a `CardDetailsResult` (success + `CardDetails`, or a
`CardServiceErrorCode` on failure) instead of `null`, so the controller can map
failures to structured `ApiErrorResponse` payloads with proper HTTP status codes
(400/404/503/500).

### CardDataApi (the backend microservice)

**This project is not required for the solution to work.** `CardServiceMock` is
fully sufficient to run and demo the rule engine end-to-end on its own. CardDataApi
was added purely to **demonstrate a realistic two-microservice setup**: a separate
backend service owning its own data (`CardDataProvider`, in-memory sample cards) and
exposing it over HTTP via `CardInfoController`, decoupled from the façade.

Its main purpose is to showcase **OpenAPI-based client generation** end-to-end: a
real backend contract that the façade consumes through a generated client rather
than a hand-written HTTP wrapper, the same way you'd integrate with any other
team's/service's OpenAPI-described API in a real microservices architecture.

## OpenAPI client generation (how the two projects stay in sync)

This is a two-stage, mostly-automatic pipeline:

1. **CardDataApi -> OpenAPI document.** CardDataApi uses Swashbuckle
   (`AddSwaggerGen`/`UseSwagger`) to generate its OpenAPI document from its
   controllers and DTOs. After every build of `CardDataApi`, an MSBuild target
   (`RegenerateCardDataApiSwagger` in `CardDataApi.csproj`) runs the Swashbuckle CLI
   tool against the freshly built `CardDataApi.dll` (no running server required) and
   writes the result to
   `CardMicroserviceApi/OpenApiSpecs/CardDataApi.swagger.json`. This keeps the
   committed contract snapshot automatically in sync with CardDataApi's actual
   controllers/DTOs.
   - The Swashbuckle CLI tool is a local dotnet tool, declared in
	 `.config/dotnet-tools.json`. Run `dotnet tool restore` once after cloning so
	 it's available on your machine/CI.

2. **OpenAPI document -> generated C# client.** `CardMicroserviceApi.csproj`
   references that swagger.json via an `<OpenApiReference>` item
   (`CodeGenerator="NSwagCSharp"`). On every build of `CardMicroserviceApi`, the
   `NSwag.ApiDescription.Client` NuGet package generates a strongly-typed client
   (`CardDataServiceClient`) plus matching DTOs/enums (`CardInfoDto`, `CardType`,
   `CardStatus`, `ApiException`, ...) directly from that file, under `obj/`.
   `CardServiceClient` uses this generated client to call CardDataApi, and
   `Mappers/CardInfoMapper` converts the transport `CardInfoDto` into the internal
   `CardDetails` domain model.

Net effect: build the solution (CardDataApi before/with CardMicroserviceApi, as a
normal solution build does) and the generated client is automatically regenerated
from CardDataApi's real contract - no manual swagger copy/paste step needed anymore.

## Running the solution

- **Rule engine only, no backend:** set `"CardDataApi:UseMock": true` in
  `CardMicroserviceApi/appsettings.json` (or Development settings) and run just
  `CardMicroserviceApi`.
- **Full two-microservice setup:** set `"CardDataApi:UseMock": false`, run
  `CardDataApi` first (so its base URL matches `CardDataApi:BaseUrl` in
  `CardMicroserviceApi`'s configuration), then run `CardMicroserviceApi`.

## Tests

`CardMicroserviceApi.Tests` covers the rule engine (`CardActionsCalculatorTests`),
the controller (`CardActionsControllerTests`), the OpenAPI-DTO-to-domain mapper
(`CardInfoMapperTests`), the HTTP error-handling wrapper
(`CardServiceResultHandlerTests`), and the input validator
(`CardRequestValidatorTests`) - all without needing either API running.
