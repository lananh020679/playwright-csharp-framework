using PlaywrightFramework.Config;
using PlaywrightFramework.Driver;

namespace PlaywrightFramework.Pages
{
    public class EmployeeDeletePage:BasePage
    {
        private const string DeleteButton ="button.btn.btn-danger";
        private static string EmailSelector(string email) => $"//dd[contains(text(),'{email}')]";
        public EmployeeDeletePage(IBrowserDriver driver, AppSettings appSettings):base(driver, appSettings){}

        protected override string RelativePath => "Employee/Delete";

        public override Task NavigateAsync()=> throw new NotSupportedException("EmployeeDeletePage URL bevat een server-assigned id. " +
                "Bereik via EmployeeListPage.DeleteEmployeeByEmailAsync(email).");
       
        public async Task<EmployeeListPage> DeleteEmployeeByEmailAsyn(string expectedEmail)
        {
            
            if(!await Driver.IsVisibleAsync(EmailSelector(expectedEmail),timeoutMs:5000))
            {
                throw new InvalidOperationException(
                     $"Delete confirmation page does not show email '{expectedEmail}'. " +
                    $"Selector mismatch of verkeerde row geklikt?");
            }
            await Driver.ClickAsync(DeleteButton);
            return new EmployeeListPage(Driver, AppSettings);
        }
     
    }
}