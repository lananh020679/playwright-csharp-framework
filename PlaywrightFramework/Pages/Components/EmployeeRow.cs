using Microsoft.Playwright;
using PlaywrightFramework.Driver;

namespace PlaywrightFramework.Pages.Components
{
    /// <summary>
    /// Representeert één rij in de werknemers-tabel, gevonden op naam.
    /// Geeft via properties de juiste CSS-selectors voor de acties in die rij,
    /// zodat de IBrowserDriver-abstractie intact blijft.
    /// </summary>
    public class EmployeeRow
    {

        private readonly IBrowserDriver _driver;
        private readonly string _employeeEmail;
        
        public EmployeeRow(IBrowserDriver browserDriver ,string employeeEmail)
        {
            _employeeEmail=employeeEmail;
            _driver = browserDriver;
        }
        /// <summary>Selector voor de rij zelf (een &lt;tr&gt; met de naam erin).</summary>
        public string RowSelector => $"tr:has(span.emp-email:has-text(\"{_employeeEmail}\"))";

        /// <summary>Selectors voor de actie-links binnen die rij.</summary>
        public string EditLinkSelector => $"{RowSelector} a.btn-edit";
        public string DetailsLinkSelector => $"{RowSelector} a.btn-detail";
        public string DeleteLinkSelector => $"{RowSelector} a.btn-del";

        /// <summary>Selector's behaviors  </summary>
        public Task ClickEditAsync(IBrowserDriver driver)
        => driver.ClickAsync(EditLinkSelector);

        public Task ClickDetailsAsync(IBrowserDriver driver)
        => driver.ClickAsync(DetailsLinkSelector);

        public Task ClickDeleteAsync(IBrowserDriver driver)
        => driver.ClickAsync(DeleteLinkSelector);
    }
}