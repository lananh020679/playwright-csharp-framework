using NUnit.Framework;
using PlaywrightFramework.Config;
using PlaywrightFramework.Driver;
using PlaywrightFramework.Pages;
using PlaywrightFramework.Pages.Components;

namespace PlaywrightTests.Tests
{
    /// <summary>
    /// Gemeenschappelijke basis voor alle testklassen.
    /// Maakt per test een verse PlaywrightDriver via PlaywrightDriverInitializer aan
    /// en wrapt de IPage in een PlaywrightBrowserDriver. Tests werken UITSLUITEND
    /// met IBrowserDriver en pagina-objecten.
    /// </summary>
    [TestFixture]
    public abstract class BaseTest
    {
        protected TestSettings Settings{get; private set;}=null!;
        protected AppSettings AppSettings{ get; private set; }=null!;
        protected ContextSettings ContextSettings{ get; private set; }=null!;

        protected PlaywrightDriver PlaywrightDriver=null!;
        protected IBrowserDriver Browser=null!;
        

        // Handige factories - geen state, dus prima om elke keer nieuw aan te maken.
        protected LoginPage Login => new LoginPage(Browser, AppSettings);
        protected HomePage Home => new HomePage(Browser, AppSettings);
        protected EmployeeListPage Employees => new EmployeeListPage(Browser, AppSettings);

        [SetUp]
        public async Task SetUp()
        {
            Settings=TestSettingsProvider.Get();
            ConfigureSettings(Settings);
            Settings.Validate();
            
            AppSettings = Settings.App;
            ContextSettings=Settings.Context;
            
            // Bouw de driver met de bestaande factories uit PlaywrightFramework.
            
            var initializer=new PlaywrightDriverInitializer(
                new BrowserFactory(Settings.Browser)
                , new BrowserContextFactory(Settings.Context)
                , new PageFactory() );
            PlaywrightDriver= await initializer.InitializeAsync();

            
            // Wrap de IPage in onze IBrowserDriver-abstractie.
            Browser=new LoggingBrowserDriver(new PlaywrightBrowserDriver(PlaywrightDriver.Page));
            TestContext.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] {TestContext.CurrentContext.Test.FullName}");

        }

        [TearDown]
        public async Task TearDown()
        {
            if(PlaywrightDriver is not null)
            {
                await PlaywrightDriver.DisposeAsync();
            }
        }

        /// <summary>
        /// Eenvoudige login-helper: navigeer naar login en log in met de default user uit AppSettings.
        /// Retourneert de NavigationBar zodat de test zelf bepaalt waar hij heen wil
        /// (GoToEmployeeListAsync, GoToHomeAsync, enz.).
        /// </summary>
        protected async Task<NavigationBar> LoginAsDefaultUserAsync()
        {
            await Login.NavigateAsync();
            return await Login.LoginWithDefaultUserAsync();
        }

        // --- Configuratie ladermethoden (placeholder: pas aan jouw eigen opzet aan) ---

        protected virtual void ConfigureSettings(TestSettings settings)
        {
            // Waarin sub_class overgeschreven worden kan

            
        } 

    }
}