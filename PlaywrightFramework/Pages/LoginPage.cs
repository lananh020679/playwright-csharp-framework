using System.Diagnostics.Contracts;
using PlaywrightFramework.Config;
using PlaywrightFramework.Driver;
using PlaywrightFramework.Pages.Components;


namespace PlaywrightFramework.Pages
{
    /// <summary>
    /// Login-pagina van eaapp.somee.com (/Account/Login).
    /// Erft van BasePage zodat navigatie en URL-opbouw consistent zijn met de rest van de site.
    /// </summary>
    public class LoginPage : BasePage
    {
        //Locators bovenaan -overzichtelijk
        protected override string RelativePath => "/Account/Login";
        private const string LogoutLink = "a[href='/Account/Logout']";
        private const string LogoutForm = "form[method='post'][action='/Account/Logout']";                                     
        private const string UserNameField ="#UserName";
        private const string PasswordField = "#Password";
        private const string LoginButton ="button.btn.btn-signin";
        // Summary error (top of form)
        private const string ErrorSummary=".validation-summary-errors";
        // Field-level errors (next to each input)
        private const string UserNameError="#UserName-error";
        private const string PasswordError="#Password-error";

        public LoginPage(IBrowserDriver driver, AppSettings appSettings): base (driver, appSettings)
        {
        }
        
        /// <summary>
        /// Vult de credentials in en klikt op Login. Retourneert de NavigationBar
        /// in plaats van een specifieke pagina - we doen geen aanname over waar
        /// de app heen redirect na inloggen. De test bepaalt zelf de volgende stap
        /// via nav.GoToEmployeeListAsync(), nav.GoToHomeAsync(), enz.
        /// </summary>
        
        public async Task<NavigationBar> LoginAsAsync(string username, string password)
        {
            await Driver.FillAsync(UserNameField, username);
            await Driver.FillAsync(PasswordField, password);
            await Driver.ClickAsync(LoginButton);

            // Wait until we either leave the login page OR an error appears
            
            try
                {
                    // ✅ chỉ 1 dòng này là đủ
                    await Driver.WaitForVisibleAsync(LogoutForm, AppSettings.Timeout);
                }
                catch (Exception)
                {
                    var error = await GetErrorMessageAsync();
                    var stillOnLogin = await IsLoginButtonVisibleAsync();
                    var url = await Driver.GetCurrentUrlAsync();

                    throw new InvalidOperationException(
                        $"Login fail. url={url}, stillOnLogin={stillOnLogin}, error='{error}'");
                }

            return new NavigationBar(Driver,AppSettings);
        }
        public async Task<string> GetUserNameErrorAsync()
        {
            if(!await Driver.IsVisibleAsync(UserNameError)) return string.Empty;
            return await Driver.GetTextAsync(UserNameError);
        }
            
        public async Task<string> GetPasswordErrorAsync()
        {
            if(!await Driver.IsVisibleAsync(PasswordError)) return string.Empty;
            return await Driver.GetTextAsync(PasswordError);
        }
            
        /// <summary>Convenience: login met de credentials uit AppSettings.</summary>
        public Task<NavigationBar> LoginWithDefaultUserAsync()
            => LoginAsAsync(AppSettings.Username, AppSettings.Password);
 
        /// <summary>Geeft de zichtbare foutmelding terug (lege string als er geen is).</summary>
        public async Task<string> GetErrorMessageAsync()
        {
            if (!await Driver.IsVisibleAsync(ErrorSummary)) return string.Empty;
            return await Driver.GetTextAsync(ErrorSummary);
        }
 
        /// <summary>True als de loginknop op het scherm staat (snel-controle of we wel op LoginPage zijn).</summary>
        public Task<bool> IsLoginButtonVisibleAsync() => Driver.IsVisibleAsync(LoginButton);
    }
}