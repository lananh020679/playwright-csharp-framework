using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using PlaywrightTests.TestData;


namespace PlaywrightTests.Tests
{
    public abstract partial class BaseTest
    {
        // Instance-level (NIET static) -> parallel-safe.
        // Met ParallelScope.Fixtures: elke fixture instance heeft eigen lijst.
        private readonly List<Func<Task>> _cleanupActions  = new();

        /// <summary>
        /// Registreer een cleanup action. Wordt uitgevoerd in TearDown
        /// in LIFO order (laatst geregistreerd = eerst uitgevoerd).
        ///
        /// Cleanup failures worden gelogd maar maken de test NIET rood.
        /// Een echte test failure overschrijft NIET door cleanup exception.
        /// </summary>
        protected void RegisterCleanup(Func<Task> cleanupAction)
        {
            ArgumentNullException.ThrowIfNull(cleanupAction);
            _cleanupActions.Add(cleanupAction);
        }

        /// <summary>
        /// Helper voor de meest voorkomende case: delete employee na test.
        /// Gebruik session cookie van huidige test - GEEN re-login (perf).
        /// </summary>
        /// <example>
        /// var emp = EmployeeBuilder.NewUnique("edit");
        /// RegisterEmployeeCleanup(emp);   // <-- registreer DIRECT na bouw
        /// // ... rest van test logic ...
        /// </example>
        protected void RegisterEmployeeCleanup(EmployeeData employee)
        {
            ArgumentNullException.ThrowIfNull(employee);
            RegisterCleanup(async ()=>
            {
                await Employees.NavigateAsync();
                await Employees.WaitForReadyAsync();

                if (await Employees.HasEmployeeAsync(employee.Email))
                {
                    // Verwacht: DeleteEmployeeByEmailAsync toegevoegd aan EmployeeListPage.
                    // Zie EmployeeListPage.EXTENSIONS.md.
                    var confirm =await Employees.DeleteEmployeeByEmailAsync(employee.Email);
                    await confirm.DeleteEmployeeByEmailAsyn(employee.Email);
                }
            });
        }


        protected async Task RunRegisteredCleanupsAsync()
        {
            try
            {
                // LIFO via reverse index (lichter dan .AsEnumerable().Reverse()).
                for (int i = _cleanupActions.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        await _cleanupActions[i]();
                    }
                    catch (Exception ex)
                    {
                        // Log marker consistent met [RETRY-RESULT] -> CI parsers
                        // kunnen beide picken up met dezelfde regex.
                        TestContext.WriteLine(
                            $"[CLEANUP-FAILED] {TestContext.CurrentContext.Test.FullName} | " +
                            $"{ex.GetType().Name}: {ex.Message}");
                    }
                }
            }
            finally
            {
                // ALTIJD clearen - ook na exception in log call zelf.
                // Voorkomt leak naar volgende test in dezelfde fixture instance.
                _cleanupActions.Clear();
            }
        }
    }
    
}