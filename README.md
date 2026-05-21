# Playwright C# Framework Demo

![CI](https://github.com/lananh020679/playwright-csharp-framework/actions/workflows/ci.yml/badge.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/License-MIT-green.svg)

Demonstration of a maintainable test automation framework using Playwright and
NUnit, built around the Employee management demo app at [eaapp.somee.com](http://eaapp.somee.com).

## Highlights

- **Locale-independent selectors** — uses IDs, hrefs, and structural locators only
- **Data-driven tests** — `[TestCaseSource]` with DTO + JSON sources
- **Parallel-safe by design** — unique data per test via factory
- **Fluent Page Object Model** — chained navigation, no selector leakage in tests
- **GitHub Actions CI** — every push triggers test run
- **Playwright tracing** — failure artifacts uploaded for debugging

## Quick start

```bash
git clone <repo>
cd <repo>
dotnet user-secrets set "App:Password" "<your password>" --project PlaywrightTests
dotnet test
```

## Project structure

```
PlaywrightFramework/       # Library: Pages, Driver, Config
PlaywrightTests/           # Test project: NUnit tests, test data
.github/workflows/ci.yml   # CI config
docs/adr/                  # Architecture Decision Records
```

## Design decisions

See [docs/adr/](docs/adr/) for architecture decision records on:
- Why Page Object Model (not Worker Object or raw locators)
- Why DDT via TestCaseSource (not TestCase for complex data)
- Why locale-independent selectors
- Why parallel-safe data factory

## Running tests

### Run all tests with default settings

```bash
dotnet test
```

### Run tests in parallel (4 workers, configured in `.runsettings`)

```bash
dotnet test --settings PlaywrightTests/.runsettings
```

### Override worker count from command line

```bash
dotnet test --settings PlaywrightTests/.runsettings -- NUnit.NumberOfTestWorkers=2
```

### Run tests against a specific environment

The framework loads `appsettings.{TEST_ENVIRONMENT}.json` on top of the base
`appsettings.json`. Default environment is `Development`.

```bash
# Linux/macOS
TEST_ENVIRONMENT=CI dotnet test

# Windows PowerShell
$env:TEST_ENVIRONMENT="CI"; dotnet test
```

Available environments:
- `Development` — headed browser, `SlowMo=300` for visual debugging
- `CI` — headless, video recording enabled

### Run a single test or test class

```bash
dotnet test --filter "FullyQualifiedName~LoginTests"
dotnet test --filter "Name=Login_with_valid_credentials_super_trace"
```

### Configure secrets

Password is never committed. Use one of:

```bash
# Option 1: dotnet user-secrets (local dev)
dotnet user-secrets set "App:Password" "<password>" --project PlaywrightTests

# Option 2: environment variable (CI, scripts)
# Linux/macOS
export App__Password="<password>"
# Windows PowerShell
$env:App__Password="<password>"
```

Note: `__` (double underscore) is the .NET convention for mapping environment
variables to nested config keys (`App:Password` -> `App__Password`).

### View test results

After running, results are written to `PlaywrightTests/TestResults/`:
- `test-results.trx` — machine-readable, opens in Visual Studio
- `test-results.html` — browser-readable report
- `videos/` (CI only) — Playwright video recordings

## License

MIT
