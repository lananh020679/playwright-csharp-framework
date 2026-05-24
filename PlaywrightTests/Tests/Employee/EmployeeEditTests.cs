using NUnit.Framework;
using PlaywrightTests.TestData;

namespace PlaywrightTests.Tests.Employee
{
    [TestFixture]
    public class EmployeeEditTests:BaseTest
    {
        [Test]
        [Category(TestCategories.Regression)]
        public async Task Edit_employee_name_should_persist_after_save()
        {
            // Arrange - maak een employee speciaal voor deze test
            var initial = EmployeeBuilder.NewUnique("edit-name");
            await RegisterEmployeeCleanup(initial);          // <-- ná login
            var nav = await LoginAsDefaultUserAsync();
            var list = await nav.GoToEmployeeListAsync();
            var form = await list.ClickCreateNewAsync();

            await form.FillAsync(initial.Name, 
                initial.Age, 
                initial.Salary, 
                initial.DurationWorked, 
                initial.Grade, 
                initial.Email);
            list = await form.SubmitAsync();

            await list.WaitForReadyAsync();           // wacht tot tabel zichtbaar

            Assert.That(await list.HasEmployeeAsync(initial.Email), Is.True,
            "Setup mislukt: employee moet eerst bestaan voor edit-test.");

            var editForm = await list.EditEmployeeAsync(initial.Email);
            var newName= initial.Name + "_EDITED";
            await editForm.FillAsync(newName,
                initial.Age, 
                initial.Salary, 
                initial.DurationWorked, 
                initial.Grade, 
                initial.Email);
            list= await editForm.SubmitAsync();
            // Assert - employee bestaat nog (zelfde email) en list refresht ok
            Assert.That(await list.HasEmployeeAsync(initial.Email), Is.True,
                $"Employee met email '{initial.Email}' moet nog bestaan na rename.");

        }
    }
}