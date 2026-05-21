using PlaywrightFramework.Config;
using PlaywrightFramework.Driver;
using PlaywrightFramework.Pages.Components;

namespace PlaywrightFramework.Pages
{
    /// <summary>
    /// Lijst van werknemers (/Employee). Werkt samen met EmployeeRow voor rij-acties
    /// en met EmployeeFormPage voor het aanmaken/bewerken.
    /// </summary>
    public class EmployeeListPage : BasePage
    {
        private const string CreateNewLink = "a[href='/Employee/Create']";
        private const string EmployeeTableBody ="table.table-borderless tbody";
        private const string TableRows = "table tbody tr";
        public EmployeeListPage(IBrowserDriver driver, AppSettings appSettings):base(driver, appSettings){}
        protected override string RelativePath => "/Employee";
        
        public NavigationBar Nav => new NavigationBar(Driver,AppSettings);

         /// <summary>Aantal werknemers dat in de tabel staat.</summary>
        public Task<int> CountEmployeesAsync()=> Driver.GetCountAsync(TableRows);
        
        /// <summary>True als een rij met deze naam voorkomt in de tabel.</summary>
        public async Task<bool> HasEmployeeAsync(string email)
        => await Driver.IsVisibleAsync(new EmployeeRow(Driver, email).RowSelector, timeoutMs: 5000);
        /// <summary>Klikt op de Edit-link in de rij van de gegeven werknemer.</summary>
        public async Task<EmployeeFormPage> EditEmployeeAsync(string name)
        {
            var row = new EmployeeRow(Driver, name);
            await Driver.ClickAsync(row.EditLinkSelector);
            return new EmployeeFormPage(Driver, AppSettings);
        }
        public async Task<EmployeeFormPage> ClickCreateNewAsync()
        {
            await Driver.ClickAsync(CreateNewLink);
            return new EmployeeFormPage(Driver, AppSettings);
        }

        public async Task WaitForReadyAsync()
        {
            await Driver.WaitForVisibleAsync(EmployeeTableBody);
        }

    }
        
}