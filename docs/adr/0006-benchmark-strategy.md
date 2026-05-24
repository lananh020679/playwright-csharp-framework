# ADR 0006 - Benchmark strategy: measure, don't assert

- **Status**: Accepted
- **Date**: 2026-05-24
- **Deciders**: Lan Anh
- **Supersedes**: implicit "parallel must be faster" assertion in AsyncBrowserTests v1
- **Related**: ADR 0003 (parallel-safety), ADR 0005 (retry-strategy), Plan W7

## Context

`AsyncBrowserTests.Parallel_Is_Faster_Than_Sequential` was originally written with the
assertion that 3 browsers launched in parallel must finish faster than 3 browsers
launched sequentially:

```csharp
Assert.That(parallelTime, Is.LessThan(sequentialTime));
```

After moving the test to `Benchmarks/` (Plan W7) and running it, the test FAILED:

```
Sequentieel: 1574 ms
Parallel:    3476 ms   (2.2x SLOWER)
Expected: less than 1574, but was 3476
```

This forced an honest review: **the assertion was based on a wrong assumption.**

### Why parallel was slower (the analysis)

1. **Playwright .NET driver pipe is single-channel.** The C# library talks to a Node.js
   driver process via a single pipe. `Task.WhenAll(3 launches)` does not give true
   parallelism - the Launch commands are serialized over that single pipe.

2. **Browser launch is CPU-bound, not I/O-bound.** `async/await` only wins when the
   task waits on I/O. Launching Chromium = spawn process + load DLLs + initialise V8
   = pure CPU work. 3 concurrent launches fight for the same CPU/RAM/disk and total
   time grows.

3. **OS file cache biased the sequential measurement.** Browser 1 was cold start
   (Chromium binary loaded from disk). Browsers 2 and 3 were warm (already in RAM).
   So sequential's ~525 ms/browser average is misleadingly fast. Parallel can't
   exploit warm-cache because all 3 start cold simultaneously.

4. **count = 3 is too small.** Task scheduling overhead + resource contention >
   parallelism benefit at low counts. Parallel only "wins" when N is large enough
   that I/O waits overlap.

### Why this test still has value

Despite the wrong assertion, the test demonstrates two things worth keeping in the
portfolio:

- The framework **can** launch multiple browsers concurrently without crashing
  (no shared-state races, no deadlock). That's the real parallel-safety claim
  (covered by ADR 0003).
- A junior who realised their benchmark assumption was wrong, documented why,
  and refactored it - that's a stronger portfolio signal than any green benchmark.

## Decision

**Change benchmarks from assertion-based to measurement-based:**

1. The test **measures** sequential and parallel timing and logs them to
   `TestContext`. It does NOT assert that parallel < sequential.
2. The test asserts only against **catastrophic** regression: parallel must not be
   slower than `sequential * 10`. Anything within that window is "normal overhead".
3. The test is in `Benchmarks/` with `[Category("Benchmark")]`. CI default uses
   `--filter "TestCategory!=Benchmark"`, so a benchmark slow-down does NOT block
   shipping.
4. Manual benchmark runs (`dotnet test --filter "TestCategory=Benchmark"`) print
   numbers for human review.
5. For real performance regression tracking, use **BenchmarkDotNet** in a separate
   project. NUnit + Stopwatch is not a benchmark framework - it's a measurement
   helper at best.

### Why NOT just delete the test

- Deleting would lose a small but real demonstration of parallel-safety.
- The ADR + telemetry test together show engineering maturity: "I learned why my
  first instinct was wrong, here's what I'd do differently."

### Why NOT add BenchmarkDotNet now

- Adding a separate project + new dependency for a single benchmark is over-engineering
  at current portfolio scope. Documented as future work (see Implementation below).

## Alternatives considered

### A. Hard assertion with tolerance: `parallel < sequential * 1.5`

**Rejected**: still flaky on weak CI runners or under noisy neighbours. A 50% margin
is not principled - either we believe parallel should be faster (it's not, see #1-#4
above) or we don't.

### B. Delete the benchmark entirely

**Rejected**: see "Why NOT just delete" above.

### C. Rewrite in BenchmarkDotNet now

**Rejected for now, accepted as future work**: BenchmarkDotNet is the right tool for
benchmarks (warm-up, multiple iterations, statistical analysis, ASCII tables). Adding
a separate project is out of scope for this portfolio iteration but should be done
before measuring real performance regressions.

## Consequences

### Positive

- CI is no longer broken by an arbitrary timing assumption.
- Reviewers see numbers, draw their own conclusions, and read the ADR for context.
- The ADR itself is portfolio gold - it documents a learning moment.
- No new dependencies.

### Negative

- The test no longer "fails" if parallel-safety breaks (only catastrophic 10x slowdown
  triggers fail). Real parallel-safety is verified instead by ADR 0003's actual
  functional tests (`ParallelScope.Fixtures`, immutable config records).
- Without BenchmarkDotNet, the measurement is single-run and somewhat noisy. Use it
  as a smoke check, not as a perf regression source of truth.

### Neutral

- Anyone wanting "real" benchmark numbers must follow up by adding a BenchmarkDotNet
  project. ADR documents this as planned future work.

## Implementation checklist

- [x] Remove `Is.LessThan(sequentialTime)` assertion from AsyncBrowserTests.
- [x] Add WarmUp() step to eliminate cold-start bias.
- [x] Increase count from 3 to 5 to give parallelism a chance.
- [x] Log measurements via `TestContext.WriteLine` (visible in TRX + Allure).
- [x] Soft assertion: `parallel < sequential * 10` (catches deadlock, not normal overhead).
- [x] Keep `[Category("Benchmark")]` so CI default excludes it.
- [ ] (Future) Create `PlaywrightBenchmarks` project using BenchmarkDotNet.
- [ ] (Future) Wire BenchmarkDotNet output into a `bench/` artifact in CI.

## References

- Original failing run output (in repo history before this ADR was written).
- BenchmarkDotNet docs: https://benchmarkdotnet.org
- Playwright .NET driver architecture: https://playwright.dev/dotnet/docs/api/class-playwright
- ADR 0003 - Parallel-safe configuration caching (the real parallel-safety claim).
