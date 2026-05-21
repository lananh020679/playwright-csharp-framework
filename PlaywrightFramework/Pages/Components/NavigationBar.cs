using System.Formats.Tar;
using PlaywrightFramework.Config;
using PlaywrightFramework.Driver;

namespace PlaywrightFramework.Pages.Components
{
    /// <summary>
    /// Herbruikbare navigatiebalk die op vrijwel elke pagina van eaapp.somee.com verschijnt.
    /// Niet een BasePage zelf - dit is een component dat door pagina-objecten wordt blootgesteld.
    /// </summary>
    public class NavigationBar
    {
        private readonly IBrowserDriver _driver;
        protected readonly AppSettings _appSettings;

        // Selectors gebaseerd op de tekst in de top-navigatie van eaapp.somee.com.
        private const string LoginLink="a[href='/Account/Login']";
        private const string LogoutLink="a[href='/Account/Logout']";
        private const string EmployeeListLink = "a[href='/Employee']";
        private const string AboutLink        = "a[href='/Home/About']";
        private const string HomeLink         = "a[href='/']";
        private const string RegisterLink     = "a[href='/Account/Register']";

        public NavigationBar(IBrowserDriver driver, AppSettings appSettings )
        {
            _driver=driver;
            _appSettings=appSettings;
        }

        public Task GoToLoginAsync() => _driver.ClickAsync(LoginLink);
        public async Task<EmployeeListPage> GoToEmployeeListAsync() 
        { 
            await _driver.ClickAsync(EmployeeListLink);
            return new EmployeeListPage(_driver, _appSettings);
        }
        public Task GoToAboutAsync()         => _driver.ClickAsync(AboutLink);
        public Task GoToHomeAsync()          => _driver.ClickAsync(HomeLink);
        public Task GoToRegisterAsync()      => _driver.ClickAsync(RegisterLink);
        public Task LogoutAsync()            => _driver.ClickAsync(LogoutLink);

        /// <summary>True als de gebruiker is ingelogd (Log off knop is zichtbaar).</summary>
        public Task<bool> IsLoggedInAsync() => _driver.IsVisibleAsync(LogoutLink);

    }
}