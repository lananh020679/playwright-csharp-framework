# 0004 - Parallel-Safe Data Factory

**Status**: Accepted
**Date**: 2026-05-20

## Context

The test suite runs with `ParallelScope.Fixtures` and 4 NUnit workers (see
`.runsettings`). Tests share several resources at runtime:

- Configuration loaded from `appsettings.json` + `appsettings.{env}.json`
  via `TestSettingsProvider` (a static class).
- Browser/page objects created per test by `BaseTest.SetUp`.
- Test input data declared in classes like `EmployeeTestData`,
  `LoginErrorData`, `InvalidLoginCaseSource`.

If any of these were mutable singletons, two tests on different worker threads
could mutate each other's state mid-flight, producing flaky failures that are
nearly impossible to reproduce single-threaded.

## Decision

Apply four patterns to make test data and configuration parallel-safe:

1. **Fresh browser per test** — `BaseTest.SetUp` constructs a new
   `PlaywrightDriver` for every test; `TearDown` disposes it. No browser is
   reused between tests.
2. **Cached config + Clone-on-read** — `TestSettingsProvider` holds a
   `static readonly Lazy<TestSettings>` populated once. `Get()` returns
   `_settings.Value.Clone()` so each caller gets an independent copy.
3. **Immutable test data records** — DTOs like `EmployeeData`,
   `LoginErrorCase`, `InvalidLoginCase` are C# `record` types with positional
   parameters → no public setters → cannot be mutated post-construction.
4. **Iterator-based sources** — `EmployeeTestData.ValidEmployees()` and
   `LoginErrorData.Cases()` use `yield return` to produce fresh records on
   every enumeration; no shared static collection that callers could modify.

## Rationale

- `Lazy<T>` with default thread-safety mode (`ExecutionAndPublication`)
  guarantees `Load()` runs exactly once even under concurrent first-access.
- Returning `Clone()` from `Get()` means subclasses that override
  `ConfigureSettings(settings)` to tweak browser flags only affect their own
  copy — never the cached source of truth.
- Immutable records eliminate an entire class of bug: a record cannot
  accidentally accumulate state between test runs.
- `yield return` produces values lazily and freshly per enumeration —
  there's no shared `List<T>` for tests to mutate.

## Consequences

**Positive:**
- Tests can run in parallel with no defensive copying in test code.
- Config changes inside one test don't leak to other tests.
- Adding new test data is straightforward: append a `yield return` line; no
  reasoning about concurrency needed.

**Negative:**
- `Clone()` is hand-maintained on every settings class
  (`TestSettings`, `BrowserSettings`, `ContextSettings`, `AppSettings`). If
  someone adds a property and forgets to update `Clone()`, the new property
  silently won't be cloned — subtle bug.
- Email/identifier collisions across runs: test data uses hardcoded values
  like `alice@example.com`. Within a single run this is safe (each value
  used by one case only); across runs against a stateful backend, the second
  run may fail with "already exists." Mitigation deferred — current target
  (eaapp.somee.com) is a demo site we don't control state of.
- `record`-based DTOs can't carry behavior with mutable state (rarely needed
  for test data, but a constraint worth knowing).

## Alternatives considered

- **No caching, reload config per test**: rejected — multiplies file I/O by
  the test count (~50+) for no benefit.
- **Cached config without Clone**: rejected — exactly the race condition we
  want to prevent (see Context).
- **Mutable POCOs with manual locking in tests**: rejected — pushes
  concurrency burden onto test authors; immutability removes it entirely.
- **Per-run unique data via `Guid.NewGuid()`**: considered for emails;
  deferred until a real backend (in-house app) makes idempotency necessary.

## Related

- [[0002-ddt-via-testcasesource]] — DTOs consumed by `[TestCaseSource]` rely
  on the immutability guarantee documented here.
- `.runsettings` configures `NumberOfTestWorkers=4`; `[assembly:
  Parallelizable(ParallelScope.Fixtures)]` declares the parallel scope.
