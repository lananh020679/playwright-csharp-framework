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
            RegisterEmployeeCleanup(initial);          // <-- ná login
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

            Assert.That(await list.GetRow(initial.Email).GetNameAsync(), Is.EqualTo(initial.Name),
                $"Employee met email '{initial.Email}' en '{initial.Name}' moet nog bestaan na rename.");

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
            
            Assert.That(await list.GetRow(initial.Email).GetNameAsync(), Is.EqualTo(newName),
                $"Employee met email '{initial.Email}' en '{initial.Name}' moet nog bestaan na rename.");

        }
        [Test]
        [Category(TestCategories.Regression)]
        public async Task Edit_employee_salary_should_persist()
        {
            var initial=EmployeeBuilder.NewUnique("edit-salary",salary:1000m);
            RegisterEmployeeCleanup(initial);
            var nav = await LoginAsDefaultUserAsync();
            var list=await nav.GoToEmployeeListAsync();
            var form= await list.ClickCreateNewAsync();
            await form.FillAsync(initial.Name, 
                initial.Age, 
                initial.Salary, 
                initial.DurationWorked, 
                initial.Grade, 
                initial.Email);
            list = await form.SubmitAsync();
            await list.WaitForReadyAsync();
            Assert.That(await list.GetRow(initial.Email).GetSalaryAmountAsync(), Is.EqualTo(initial.Salary),
                $"Salary moet '{initial.Salary}' zijn na create voor '{initial.Email}'.");
            // verhoog salaris
            var edit = await list.EditEmployeeAsync(initial.Email);
            decimal newSalary = 6900m;
            await edit.FillAsync(initial.Name,
                initial.Age,
                newSalary,
                initial.DurationWorked,
                initial.Grade,
                initial.Email);
            list = await edit.SubmitAsync();
            await list.WaitForReadyAsync();
            Assert.That(await list.GetRow(initial.Email).GetSalaryAmountAsync(), Is.EqualTo(newSalary),
                $"Salary moet '{newSalary}' zijn na edit voor '{initial.Email}'.");
            
        }
    }
}