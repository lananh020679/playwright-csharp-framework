using NUnit.Framework;
using PlaywrightFramework.Pages;
using PlaywrightTests.TestData;

namespace PlaywrightTests.Tests.Employee
{
    public class EmployeeCreateDataDrivenTests : BaseTest
    {
        [TestCaseSource(typeof(EmployeeTestData), nameof(EmployeeTestData.ValidEmployees))]
        public async Task Create_employee_should_appear_in_list(EmployeeData data)
        {
            //Login -> navigate
            var nav =await LoginAsDefaultUserAsync();
            var list= await nav.GoToEmployeeListAsync();
            var form= await list.ClickCreateNewAsync();
            
            // Act- do data vao form
            await form.FillAsync(data.Name,data.Age,data.Salary,data.DurationWorked,data.Grade, data.Email);
            var listAfter =await form.SubmitAsync();
            
            //Assert
            Assert.That(await listAfter.HasEmployeeAsync(data.Email), Is.True, $"Employee '{data.Email}' phải xuất hiện trong list sau khi tạo.");

        }
    }
}