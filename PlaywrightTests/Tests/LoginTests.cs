using NUnit.Framework;

namespace PlaywrightTests.Tests
{
    /// <summary>
    /// Tests rond inloggen. Demonstreren: navigatie, formulier-invoer, foutpadt.
    /// </summary>
    public class LoginTests : BaseTest
    {
        [Test]
        public async Task Login_with_valid_credentials_super_trace()
        {
            const string LogoutExact = "a[href='/Account/Logout']";
            const string LogoutAnyHref = "a[href*='Logout']";
            const string ErrorSummary = ".validation-summary-errors";
            const string LoginButton = "button.btn.btn-signin";

            await Login.NavigateAsync();

            TestContext.WriteLine($"[BEFORE] url={await Browser.GetCurrentUrlAsync()}");
            TestContext.WriteLine($"[BEFORE] title={await Browser.GetTitleAsync()}");

            try
            {
                // Dit faalt nu bij de WaitForVisibleAsync in LoginAsAsync
                await Login.LoginAsAsync(AppSettings.Username, AppSettings.Password);
                TestContext.WriteLine("[LOGIN] LoginAsAsync returned (no timeout)");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[EXCEPTION] {ex.GetType().Name}: {ex.Message}");

                // --- Evidence snapshot na failure ---
                var url = await Browser.GetCurrentUrlAsync();
                var title = await Browser.GetTitleAsync();
                var errorText = await Login.GetErrorMessageAsync();
                var stillOnLogin = await Login.IsLoginButtonVisibleAsync();

                TestContext.WriteLine($"[AFTER-FAIL] url={url}");
                TestContext.WriteLine($"[AFTER-FAIL] title={title}");
                TestContext.WriteLine($"[AFTER-FAIL] errorText='{errorText}'");
                TestContext.WriteLine($"[AFTER-FAIL] stillOnLogin={stillOnLogin}");

                TestContext.WriteLine($"[PROBE] errorSummaryVisible={await Browser.IsVisibleAsync(ErrorSummary)}");
                TestContext.WriteLine($"[PROBE] loginButtonVisible={await Browser.IsVisibleAsync(LoginButton)}");
                TestContext.WriteLine($"[PROBE] logoutCountExact={await Browser.GetCountAsync(LogoutExact)}");
                TestContext.WriteLine($"[PROBE] logoutCountAnyHref={await Browser.GetCountAsync(LogoutAnyHref)}");

                throw; // rethrow zodat de test rood blijft (bewust)
            }
        }
       
    }
}