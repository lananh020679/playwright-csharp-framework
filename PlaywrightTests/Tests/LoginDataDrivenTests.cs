using NUnit.Framework;
using PlaywrightFramework.Pages.Components;

namespace PlaywrightTests.Tests
{
    public record LoginErrorCase
    (
        string Username,
        string Password,
        string? ExpectedUserNameError,
        string? ExpectedPasswordError,
        string? ExpectedSummaryError,
        string Scenario
    )
    {
        public override string ToString()=> Scenario;
    }
    public static class LoginErrorData
    {
        public static IEnumerable<LoginErrorCase> Cases()
        {
            yield return new (
                Username: "", Password: "",
                ExpectedUserNameError: "required",
                ExpectedPasswordError: "required",
                ExpectedSummaryError: null,
                Scenario: "Empty -> 2 field-level required errors"
            );
            yield return new (
                Username: "admin", Password: "wrong",
                ExpectedUserNameError: null,
                ExpectedPasswordError: null,
                ExpectedSummaryError: "Invalid",
                Scenario: "Wrong password -> summary error"
            );
            yield return new (
            Username: "notexist", Password: "x",
            ExpectedUserNameError: null,
            ExpectedPasswordError: null,
            ExpectedSummaryError: "Invalid",
            Scenario: "Unknown user -> summary error");
        }
            
    }
    public class LoginDataDrivenTests:BaseTest
    {
        [TestCaseSource(typeof(LoginErrorData), nameof(LoginErrorData.Cases))]
        public async Task Invalid_login_shows_expected_errors(LoginErrorCase c)
        {
            await Login.NavigateAsync();
            Assert.ThrowsAsync<InvalidOperationException>(async() => await Login.LoginAsAsync(c.Username, c.Password));

            if (c.ExpectedUserNameError is not null)
                Assert.That(await Login.GetUserNameErrorAsync(),
                    Does.Contain(c.ExpectedUserNameError).IgnoreCase);

            if (c.ExpectedPasswordError is not null)
                Assert.That(await Login.GetPasswordErrorAsync(),
                    Does.Contain(c.ExpectedPasswordError).IgnoreCase);

            if (c.ExpectedSummaryError is not null)
                Assert.That(await Login.GetErrorMessageAsync(),
                    Does.Contain(c.ExpectedSummaryError).IgnoreCase);
        }
        // Ba dong [Testcase] = ba testcases doc lap.
        // Tham so cuoi la expected: thong bao loi mong doi
        [TestCase("admin", "wrong", "Invalid", TestName = "Login_wrong_password_shows_invalid_error")]
        [TestCase("notexist","x","Invalid",TestName ="Login_unknown_user_shows_invalid_error")]
        public async Task Login_with_invalid_credentials_shows_error(string username, string password, string expectedErrorSubstring)
        {
            await Login.NavigateAsync();
            try
            {
                await Login.LoginAsAsync(username, password);
                Assert.Fail("Login lẽ ra phải thất bại nhưng đã thành công.");
            }
            catch(InvalidOperationException)
            {
                
            }
            // Assert — kiểm tra thông báo lỗi
            var error =await Login.GetErrorMessageAsync();
            Assert.That(error, Does.Contain(expectedErrorSubstring).IgnoreCase,$"Mong thấy '{expectedErrorSubstring}' trong lỗi, nhưng nhận: '{error}'");
            
        }

    }
    

}