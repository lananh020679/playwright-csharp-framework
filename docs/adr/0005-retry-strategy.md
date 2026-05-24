# ADR 0005 - Retry strategy for flaky tests

- **Status**: Accepted
- **Date**: 2026-05-24
- **Deciders**: Lan Anh
- **Related**: ADR 0003 (parallel-safety), Portfolio Improvement Plan W2

## Context

End-to-end UI tests run against a live external app (`eaapp.somee.com`). Real-world
E2E suites have a baseline flake rate of ~1-3% caused by factors outside the test's
control: network latency, server cold-start, animation timing, browser warm-up,
slow DOM hydration after navigation.

Without a retry mechanism every transient failure becomes a red CI run. Engineers
quickly start ignoring "the noisy E2E job", which destroys the signal value of the
suite. Hiring managers reviewing this portfolio will expect to see how this trade-off
is handled - a framework with zero retry mechanism signals inexperience with
production test suites.

At the same time, retrying every test indiscriminately hides real regressions and
doubles CI cost. We need a targeted strategy.

## Decision

We adopt **selective per-test retry via NUnit `[Retry]` attribute combined with a
`Flaky` category**:

1. Tests that are observed to be intermittently flaky after >=3 runs get tagged with
   `[Category(TestCategories.Flaky)]` AND `[Retry(2)]`.
2. Tests in the `Smoke`, `Api`, and `Visual` categories never use `[Retry]`. A flake
   there indicates a real issue and must be debugged, not papered over.
3. Every retried test logs a `[RETRY-RESULT]` line in `TestContext` so the CI report
   shows how many retries were needed. A test that passes only on retry 2-of-2 is a
   warning, not a green check.
4. Maximum retry count is **2** (i.e. test runs at most 3 times). Beyond this, the
   test is either truly broken or the flakiness exceeds acceptable budget and must
   be fixed at source.

### What we explicitly do NOT retry

- **Login tests** - if login fails, every following test will also fail, masking
  retry signal.
- **Data integrity tests** - "create then verify" tests must catch real backend
  bugs, not be retried into a false green.
- **Assertions that check counts/sums** - retry hides race conditions we want to
  see.

## Alternatives considered

### A. Global retry on every test
```csharp
[assembly: Retry(2)]
```

**Rejected**: hides real regressions, doubles CI time on a failed run.
Hiring managers reviewing the framework would correctly flag this as anti-pattern.

### B. Custom RetryAttribute with smart delay/backoff
**Rejected** for now: NUnit's built-in `[Retry]` is enough at current suite size
(~20 tests). Custom retry adds complexity without measurable gain. Will revisit
if suite grows past 100 tests.

### C. Test-framework agnostic retry via Polly
**Rejected**: Polly excels at retrying production HTTP calls inside SUT code, not
at retrying NUnit test methods. Using Polly here would require manual try/catch in
every test - boilerplate explosion.

## Consequences

### Positive

- Real CI signal preserved: a red run still means something broke.
- Reviewers see retry-discipline (selective, logged, capped at 2) - this is a
  senior-level concern handled visibly.
- ADR documents the trade-off explicitly so future contributors can't blanket-add
  retry without revisiting this decision.

### Negative

- New tests need a manual decision: "is this Flaky-category?". Without good
  judgment a developer can mark everything Flaky and erode signal.
  - **Mitigation**: ADR + code-review checklist item ("if you added `Flaky`,
    justify in PR description").
- Retry doubles CI minutes for any failing flaky test. Acceptable at suite size
  <100. Revisit at scale.
- `[Retry-RESULT]` lines clutter the test output. Filter in IDE if distracting.

### Neutral

- NUnit `[Retry]` re-runs ONLY the [Test] body. `[SetUp]` and `[TearDown]` run on
  each retry, which is what we want (fresh browser per attempt).

## Implementation checklist

- [ ] Add `PlaywrightTests/Tests/TestCategories.cs` (central category names).
- [ ] In `BaseTest.cs` add `partial` keyword and call `LogRetryStatus()` from
      existing TearDown.
- [ ] Identify the 2-3 most flaky tests from CI history; tag them
      `[Category(TestCategories.Flaky)] [Retry(2)]`.
- [ ] Update `.github/workflows/ci.yml` so flake retries don't extend timeout.
- [ ] Add note to CONTRIBUTING.md: "use Retry sparingly, justify in PR".

## References

- NUnit Retry: https://docs.nunit.org/articles/nunit/writing-tests/attributes/retry.html
- Google Testing Blog: "Flaky tests" - the cost of un-managed flakiness.
- ADR 0003 - Parallel-safe configuration caching (related context).
