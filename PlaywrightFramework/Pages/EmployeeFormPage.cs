using PlaywrightFramework.Config;
using PlaywrightFramework.Driver;

namespace PlaywrightFramework.Pages
{
    /// <summary>
    /// Formulier-pagina voor zowel Create (/Employee/Create) als Edit (/Employee/Edit/{id}).
    /// Beide schermen hebben dezelfde velden, dus één pagina-object volstaat.
    /// </summary>
    public class EmployeeFormPage : BasePage
    {
        private const string NameField ="#Name";
        private const string AgeField ="#Age";
        private const string SalaryField ="#Salary";
        private const string DurationField ="#DurationWorked";
        private const string GradeSelect ="#Grade";
        private const string EmailField ="#Email";
        
        // Knoptekst kan 'Create' of 'Save' zijn, afhankelijk van Create vs Edit.
        private const string SubmitButton="button.btn-submit";
        public EmployeeFormPage(IBrowserDriver driver, AppSettings appSettings) : base (driver, appSettings){}
        //De URL hangt af van Create vs Edit, dus voor IsAtPageAsync gebruiken we een ruimer pad.
        protected override string RelativePath => "/Employee";

        /// <summary>Vul alle velden in op basis van een EmployeeData DTO.</summary>
        public async Task FillAsync(string name, int age, decimal salary, int durationWorked, string grade, string email)
        {
            await Driver.FillAsync(NameField,name);
            await Driver.FillAsync(AgeField,age.ToString());
            await Driver.FillAsync(SalaryField, salary.ToString(System.Globalization.CultureInfo.InvariantCulture));
            await Driver.FillAsync(DurationField, durationWorked.ToString());
            await Driver.SelectByValueAsync(GradeSelect, grade);
            await Driver.FillAsync(EmailField,email);
        }

        /// <summary>Verzend het formulier en navigeer terug naar de werknemerslijst.</summary>
        public async Task<EmployeeListPage> SubmitAsync()
        {
            await Driver.ClickAsync(SubmitButton);
            var list =new EmployeeListPage(Driver, AppSettings);
            // chờ bảng list render xong
            await list.WaitForReadyAsync();
            return list;
        }
        //public async Task 


    }
}