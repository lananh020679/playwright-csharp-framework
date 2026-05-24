namespace PlaywrightTests.Tests
{
    public static class TestCategories
    {
        /// <summary>
        /// Tests die af en toe falen door netwerk / animatie timing.
        /// Krijgen [Retry(2)] zodat ze tot 2x retry voor echte failure.
        /// Reden voor retry-mechanisme: zie ADR 0005-retry-strategy.md.
        /// </summary>
        public const string Flaky="Flaky";
        
        /// <summary>Snelle smoke tests - draait op elke PR.</summary>
        public const string Smoke="Smoke";
        /// <summary>Volledige regression suite - draait nightly.</summary>
        public const string Regression ="Regression";

        /// <summary>Visual regression - alleen op Linux CI (W5).</summary>
        public const string Visual="Visual";

        /// <summary>API tests - sneller, geen browser (W6).</summary>
        public const string Api="Api";
        
        /// <summary>Benchmarks - uitsluiten van default CI run (W7).</summary>
        public const string Benchmark="Benchmark";
         
    }
}