# ADR 0007 - Test data cleanup via register-cleanup pattern

- **Status**: Accepted
- **Date**: 2026-05-24
- **Deciders**: Lan Anh
- **Related**: ADR 0003 (parallel-safety), ADR 0004 (parallel-safe data factory),
  ADR 0005 (retry strategy), ADR 0006 (benchmark strategy)

## Context

Tests in `EmployeeEditTests`, `EmployeeCreateDataDrivenTests` and others **create**
employees but never **delete** them. After N CI runs the eaapp.somee.com employee list
has accumulated O(N) garbage records. Two concrete problems:

1. Browser-based assertions like `await list.HasEmployeeAsync(email)` get slower as
   the list grows (more rows to scan, more pagination).
2. Visual regression baselines (ADR-relevant for W5) capture more and more
   garbage rows in screenshots.

A naive fix - add `[TearDown]` that deletes the employee - has several pitfalls
addressed below.

## Decision

Adopt a **register-cleanup pattern** layered on top of the existing single
`[TearDown]` in `BaseTest`:

1. Tests call `RegisterCleanup(Func<Task>)` (or the typed helper
   `RegisterEmployeeCleanup(EmployeeData)`) IMMEDIATELY after creating data.
2. The existing `[TearDown]` executes all registered cleanups in **LIFO** order,
   BEFORE `LogRetryStatus()` and `PlaywrightDriver.DisposeAsync()`.
3. Cleanup failures are logged via `[CLEANUP-FAILED]` marker but do **not** fail
   the test (same philosophy as ADR 0005).
4. Implementation lives in a partial file `BaseTest.Cleanup.cs` consistent with
   the `BaseTest.RetryHooks.cs` pattern.

### Execution order in [TearDown]

```
RunRegisteredCleanupsAsync()   // 1. UI cleanup - browser still alive
LogRetryStatus()               // 2. log retry/cleanup status
PlaywrightDriver.DisposeAsync() // 3. tear down browser
```

### Cleanup helpers provided

- `RegisterCleanup(Func<Task>)` - escape hatch for non-employee cleanup.
- `RegisterEmployeeCleanup(EmployeeData)` - common case, uses session cookie
  (no re-login).
- (Future, when W6 API client lands) `RegisterEmployeeApiCleanup(EmployeeData)`
  for ~10x faster cleanup via API.

## Alternatives considered

### A. Per-test `[TearDown] Cleanup()` method (separate from existing)

**Rejected (BLOCKER)**: NUnit does NOT guarantee execution order between multiple
`[TearDown]` methods in the same class. Between base and derived they run
derived → base, but two `[TearDown]`s in the same class are unordered.
A separate cleanup TearDown might run AFTER the existing one
(`PlaywrightDriver.DisposeAsync()` already called) → cleanup throws
`TargetClosedError`.

### B. Cleanup after DisposeAsync

**Rejected (MAJOR)**: UI cleanup needs `Browser`/`Page` alive. After dispose,
every cleanup action would fail. Order MUST be cleanup → dispose.

### C. OneTimeTearDown at fixture level (bulk delete)

**Considered but deferred**: cleanup once at end of fixture is less overhead than
per-test, but loses isolation. If test 3 in a fixture fails, employees from
tests 1+2 stay until fixture finishes. With `ParallelScope.Fixtures` and small
fixtures (≤5 tests), per-test cleanup is acceptable. Revisit if fixture size grows.

### D. Cleanup makes test fail

**Rejected**: violates ADR 0005's signal preservation principle. A test that
passed its assertions should be GREEN even if cleanup hiccuped. A real test
failure should not have its stack trace overwritten by a cleanup exception.

### E. Always cleanup via API

**Rejected for now, planned for later**: API cleanup is 5-10x faster than UI but
requires W6 (API client) to be complete. The framework provides a hook
(`RegisterEmployeeApiCleanup`) but for now falls back to UI cleanup. Migration
path documented in W6 ADR.

## Consequences

### Positive

- Each test cleans up its own footprint - eaapp employee list stays bounded.
- No new dependencies; pure NUnit + existing infrastructure.
- Pattern is opt-in: tests that don't need cleanup just don't call register.
- Future API cleanup is a 1-line swap (`RegisterEmployeeCleanup` →
  `RegisterEmployeeApiCleanup`).

### Negative

- Each test gains ~1-2 seconds for UI cleanup (re-navigate to /Employee, find
  row, click delete, confirm dialog). 20 tests = 20-40s CI overhead. Acceptable
  for now; mitigated when W6 API cleanup arrives.
- Tests that forget to register cleanup will leak data (no static analysis catches
  this).
  - **Mitigation**: code-review checklist item; future work could add a Roslyn
    analyzer that flags `Create*Async` without nearby `RegisterCleanup`.
- Cleanup happens even when the underlying test failed mid-flow. If the failure
  left the browser in an unexpected state (alert open, error page), cleanup
  re-navigates and recovers. Edge case: navigation itself throws → logged as
  `[CLEANUP-FAILED]`.

### Neutral

- LIFO order matches manual undo intuition: last thing you did is first thing
  to roll back.
- `_cleanupActions` is per-instance, not static. With `ParallelScope.Fixtures`
  each fixture has its own instance → parallel-safe.
- NUnit `[Retry(2)]` re-runs [Test] body. SetUp + TearDown re-run with each
  attempt. Cleanup list is cleared in `finally`, so each retry attempt has a
  fresh list. No leak between attempts.

## Implementation order (prerequisites first)

1. **Fix `EmployeeListPage.EditEmployeeAsync(string name)` bug**: takes `name`
   parameter but `EmployeeRow` selector uses email. Was incidentally hidden by
   test isolation; cleanup will trip over it. Either:
   - Rename param to `email` AND change tests to pass email, OR
   - Add new `EditEmployeeByEmailAsync(string email)` and deprecate the original.
2. **Add `EmployeeListPage.DeleteEmployeeByEmailAsync(string email)`** -
   prerequisite for `RegisterEmployeeCleanup`.
3. **Modify `BaseTest.cs` TearDown**: add `await RunRegisteredCleanupsAsync()`
   as FIRST statement.
4. **Create `PlaywrightTests/Tests/BaseTest.Cleanup.cs`** - partial with cleanup
   logic.
5. **Update test classes** (`EmployeeEditTests`, `EmployeeCreateDataDrivenTests`,
   ...): call `RegisterEmployeeCleanup(emp)` immediately after every create.
6. **Roll out gradually**: start with `EmployeeEditTests` (smallest change),
   verify on CI, then extend.

## References

- NUnit docs - SetUp/TearDown ordering:
  https://docs.nunit.org/articles/nunit/writing-tests/setup-teardown/
- ADR 0003 - Parallel-safe configuration caching.
- ADR 0004 - Parallel-safe test data factory (this builds on it).
- ADR 0005 - Retry strategy (cleanup-doesn't-fail-test follows same principle).
- W6 (API test layer) - will enable faster API cleanup.
