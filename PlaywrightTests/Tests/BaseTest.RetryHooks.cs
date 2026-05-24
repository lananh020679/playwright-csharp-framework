using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace PlaywrightTests.Tests
{
    public abstract partial class BaseTest
    {
        /// <summary>
        /// Log status retry in TestContext. NUnit doesn't expose amount of directly retrying
        /// but TestContext.CurrentContext.Result contains the latest status.
        ///
        /// with a retry-aware report:
        ///  - Status = Passed after Retry  -> log "Flaky pass (retry helped)".
        ///  - Status = Failed after Retry  -> log "Real failure (retry exhausted)".
        /// </summary>
        protected void LogRetryStatus()
        {
            var ctx=TestContext.CurrentContext; 
            var status =ctx.Result.Outcome.Status;

            // Marker for CI log parsers - regex-friendly.
            if(status==TestStatus.Failed)
            {
                TestContext.WriteLine($"[RETRY-RESULT] FAIL | {ctx.Test.FullName} | {ctx.Result.Message}");
            }
            else if(status==TestStatus.Passed)
            {
                TestContext.WriteLine($"[RETRY-RESULT] PASS | {ctx.Test.FullName}");
            }
        }
    }
}