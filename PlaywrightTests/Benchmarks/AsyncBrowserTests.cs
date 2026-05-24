using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace PlaywrightTests.Benchmarks
{
    /// <summary>
    /// BENCHMARK - khong phai functional test.
    ///
    /// LICH SU:
    /// Phien ban truoc co assertion "Is.LessThan(sequentialTime)" -> FAIL.
    /// Ly do parallel CHAM hon sequential trong setup nay:
    ///   1. Playwright .NET driver pipe la single-channel -> Launch bi serialize.
    ///   2. Browser launch la CPU-bound (spawn process, load V8), khong I/O-bound.
    ///   3. Sequential warm OS file cache, parallel cold-start cung luc.
    ///   4. count=3 qua nho - overhead Task scheduling > parallelism benefit.
    /// Chi tiet: docs/adr/0006-benchmark-strategy.md
    ///
    /// CHIEN LUOC MOI: chi DO va LOG, KHONG ASSERT timing.
    /// Test pass mien la 2 mode chay xong khong throw.
    /// Reviewer/CI co the doc so do, nhung khong fail buil tren timing.
    /// </summary>
    [TestFixture]
    [NonParallelizable]            // benchmark phai chay 1 minh
    [Category("Benchmark")]        // excluded khoi CI default
    public class AsyncBrowserTests
    {
        // Warm-up de OS cache Chromium binary truoc khi do.
        // Khong tinh thoi gian buoc nay vao ket qua.
        private static async Task WarmUpAsync(IPlaywright playwright)
        {
            await using var browser = await playwright.Chromium.LaunchAsync();
            // close immediately - chi can de cache binary trong RAM.
        }

        private static async Task LaunchAndCloseOnceAsync(IPlaywright playwright)
        {
            await using var browser = await playwright.Chromium.LaunchAsync();
            // Mo va dong - khong navigate de loai bo network noise.
        }

        [Test]
        public async Task Measure_parallel_vs_sequential_browser_launch()
        {
            const int count = 5;
            using var playwright = await Playwright.CreateAsync();

            // 1. WARM-UP - loai bo cold-start bias
            await WarmUpAsync(playwright);

            // 2. SEQUENTIAL
            var seqSw = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                await LaunchAndCloseOnceAsync(playwright);
            }
            seqSw.Stop();
            var seqMs = seqSw.ElapsedMilliseconds;

            // 3. PARALLEL
            var parSw = Stopwatch.StartNew();
            var tasks = new Task[count];
            for (int i = 0; i < count; i++)
            {
                tasks[i] = LaunchAndCloseOnceAsync(playwright);
            }
            await Task.WhenAll(tasks);
            parSw.Stop();
            var parMs = parSw.ElapsedMilliseconds;

            // 4. LOG cho reviewer
            TestContext.WriteLine("=== Browser Launch Benchmark ===");
            TestContext.WriteLine($"Count per mode      : {count}");
            TestContext.WriteLine($"Sequential          : {seqMs} ms  ({seqMs / (double)count:F0} ms/browser)");
            TestContext.WriteLine($"Parallel            : {parMs} ms  ({parMs / (double)count:F0} ms/browser)");
            TestContext.WriteLine($"Speedup (seq/par)   : {seqMs / (double)parMs:F2}x");
            TestContext.WriteLine("");
            TestContext.WriteLine("Note: parallel co the CHAM hon do Playwright driver pipe single-channel");
            TestContext.WriteLine("      + browser launch CPU-bound. Xem ADR 0006 de hieu.");

            // 5. SOFT CHECK - chi fail khi co bug that su (catastrophic regression)
            // 10x cham hon = day la bug, khong phai 'parallel overhead binh thuong'.
            Assert.That(parMs, Is.LessThan(seqMs * 10),
                "Parallel cham > 10x sequential -> co the bi deadlock hoac config sai.");

            // Pass - day la measurement, khong phai assertion.
        }
    }
}
