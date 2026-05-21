using NUnit.Framework;
using PlaywrightTests.TestData;
namespace PlaywrightTests.Tests
{
 public class LoginFromJsonTests : BaseTest
 {
    [TestCaseSource(typeof(InvalidLoginCaseSource), nameof(InvalidLoginCaseSource.Case))]
    public async Task Invalid_login_from_json(InvalidLoginCase c)
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
 }
}