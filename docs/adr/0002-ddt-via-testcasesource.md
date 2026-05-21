# 0002 - Data-Driven Tests via TestCaseSource (not TestCase) for Complex Data

**Status**: Accepted
**Date**: 2026-05-20

## Context

The framework needs to run the same test logic across multiple input sets:
invalid login combinations, valid employee creation, error message variants.
NUnit offers two main mechanisms:

1. `[TestCase(arg1, arg2, ...)]` — inline parameters per test
2. `[TestCaseSource(typeof(...), nameof(...))]` — external data source method
   returning an `IEnumerable<T>`
3. JSON/CSV-driven via `[TestCaseSource]` reading from file

The Login flow needs ~3 fields per case (username, password, expected error
messages of three different kinds, scenario name). Employee creation needs 7
fields per case (name, age, salary, duration, grade, email, scenario name).

## Decision

- Use `[TestCaseSource]` with a **C# record DTO** for cases with more than 2
  fields or with mixed nullable types.
- Use inline `[TestCase(...)]` only for trivial 2-3 primitive parameter cases
  where readability is preserved.
- For test data that may evolve outside the codebase (BA-owned, QA-owned),
  load via `[TestCaseSource]` from a **JSON file**.

## Rationale

- `[TestCase]` with 5+ parameters becomes unreadable: positional arguments lose
  meaning at the call site. A DTO with named members documents itself.
- Records are immutable by default → safe for parallel execution (see
  [[0004-parallel-safe-data-factory]]).
- `override ToString()` on the DTO gives Test Explorer a readable scenario
  name instead of `Test(arg1,arg2,...)`.
- JSON source lets non-developers contribute test cases without recompiling.

## Consequences

**Positive:**
- Test methods stay declarative — they describe behavior, not data.
- Adding a new case = adding one line to the source method or JSON file.
- Same test logic exercises N scenarios → high coverage per line of test code.

**Negative:**
- Two patterns coexist (`TestCase` for simple, `TestCaseSource` for complex) —
  developers must judge which to use.
- JSON-sourced cases lose compile-time type safety; a typo in JSON only
  surfaces at runtime.
- Discovery cost: `TestCaseSource` requires reading the source method to see
  what's covered; `TestCase` shows it inline at the test.

## Alternatives considered

- **All inline `[TestCase]`**: rejected for >3-field cases — unreadable.
- **All `[TestCaseSource]`**: rejected — overkill for 2-parameter cases like
  `Login_with_invalid_credentials_shows_error("admin", "wrong", "Invalid")`.
- **External test framework (SpecFlow/Reqnroll)**: deferred — adds
  Gherkin/BDD overhead the team doesn't currently need.

## Related

- [[0001-page-object-model]] — Page Objects consume the DTOs.
- [[0004-parallel-safe-data-factory]] — DTOs must be immutable for parallel safety.
