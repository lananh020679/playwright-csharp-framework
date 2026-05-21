using PlaywrightFramework.Config;
using PlaywrightFramework.Driver;
using PlaywrightFramework.Pages.Components;

namespace PlaywrightFramework.Pages
{
    /// <summary>
    /// Home-pagina van eaapp.somee.com. Hoofdzakelijk een launching pad
    /// voor navigatie via de NavigationBar.
    /// </summary>
    public class HomePage : BasePage
    {
        private const string  WelcomeHeading = "h1, .jumbotron h1";
        public HomePage (IBrowserDriver driver, AppSettings appSettings): base(driver,appSettings){}
        protected override string RelativePath => "/";
        public NavigationBar Nav => new NavigationBar(Driver,AppSettings);
        public Task<string> GetWelcomeTextAsync() => Driver.GetTextAsync(WelcomeHeading);
    }
}