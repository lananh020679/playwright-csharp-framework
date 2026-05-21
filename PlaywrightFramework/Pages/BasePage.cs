using System.Dynamic;
using PlaywrightFramework.Config;
using PlaywrightFramework.Driver;

namespace PlaywrightFramework.Pages
{
    /// <summary>
    /// Abstracte basis voor alle pagina-objecten.
    /// Krijgt IBrowserDriver (zodat pagina's driver-onafhankelijk blijven) en AppSettings (voor BaseUrl).
    /// Subklassen geven hun relatieve pad via <see cref="RelativePath"/>.
    /// </summary>
    public abstract class BasePage
    {
        protected readonly IBrowserDriver Driver;
        protected readonly AppSettings AppSettings;
        protected BasePage(IBrowserDriver driver, AppSettings appSettings)
        {
            Driver=driver;
            AppSettings=appSettings;
           
        }
        /// <summary>Relatief pad zonder host, bv. "/Account/Login".</summary>
        protected abstract string RelativePath{get;}

        /// <summary>Volledige URL van deze pagina (BaseUrl + RelativePath).</summary>
        protected string FullUrl => $"{AppSettings.BaseUrl.TrimEnd('/')}{RelativePath}";

        /// <summary>Open de pagina rechtstreeks via URL.</summary>
        public virtual async Task NavigateAsync()
        {
            await Driver.NavigateAsync(FullUrl);
        }

        /// <summary>Controleer of de browser zich (heuristisch) op deze pagina bevindt.</summary>
        public virtual async Task<bool> IsAtPageAsync()
        {
            var url = await Driver.GetCurrentUrlAsync();
            return url.Contains(RelativePath, StringComparison.OrdinalIgnoreCase);
        }
    }
}