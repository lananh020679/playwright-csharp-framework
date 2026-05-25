using Microsoft.Playwright;
using PlaywrightFramework.Driver;
using System.Runtime.CompilerServices;

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
        public string NameSelector => $"{RowSelector} span.emp-name";
        public string SalarySelector => $"{RowSelector} span.salary-cell";
        public string GradeSelector => $"{RowSelector} span.grade-badge";
        public string DurationWorkedSelector => $"{RowSelector} span.duration-cell";

        /// <summary>Selector's behaviors  </summary>
        public Task ClickEditAsync(IBrowserDriver driver)
        => driver.ClickAsync(EditLinkSelector);

        public Task ClickDetailsAsync(IBrowserDriver driver)
        => driver.ClickAsync(DetailsLinkSelector);

        public Task ClickDeleteAsync(IBrowserDriver driver)
        => driver.ClickAsync(DeleteLinkSelector);

        public async Task<string> GetNameAsync()
        {
            return await _driver.GetTextAsync(NameSelector);
        }
        public async Task<string> GetSalaryAsync()
        {
            return await _driver.GetTextAsync(SalarySelector);
        }

        public async Task<decimal> GetSalaryAmountAsync()
        {
            var raw = await _driver.GetTextAsync(SalarySelector);
            return decimal.Parse(
                new string([.. raw.Where(c => char.IsDigit(c) || c == '.' || c == '-')]),
                System.Globalization.CultureInfo.InvariantCulture);
        }
        public async Task<string> GetDurationWorkAsync()
        {
            return await _driver.GetTextAsync(DurationWorkedSelector);
        }
        public async Task<string> GetGradeAsync()
        {
            return await _driver.GetTextAsync(GradeSelector);
        }
    }
}