using System;
using PlaywrightTests.TestData;

namespace PlaywrightTests.TestData
{
    public static class EmployeeBuilder
    {
        /// <summary>
        /// Maakt een unieke EmployeeData. Email/Name eindigen altijd op een 8-char GUID
        /// -> 2 opeenvolgende runs botsen niet met backend constraint "email already exists".
        /// </summary>
        /// <param name="scenarioPrefix">
        /// Prefix om scenarios in log/report te onderscheiden.
        /// Vb: "edit-happy", "delete-cancel", "boundary-age0".
        /// </param>
        /// <param name="age">Override default (30) indien nodig.</param>
        /// <param name="salary">Override default (1000) indien nodig.</param>
        /// <param name="durationWorked">Override default (1) indien nodig.</param>
        /// <param name="grade">Override default ("1") indien nodig.</param>
        public static EmployeeData NewUnique(
            string scenarioPrefix="test",
            int? age=null,
            decimal? salary=null,
            int? durationWorked=null,
            string? grade=null
        )
        {
            // 8 char van GUID (zonder dash) - kort, leesbaar en uniek genoeg
            var uniek =Guid.NewGuid().ToString("N")[..8];
            return new EmployeeData(
                Name : $"{scenarioPrefix}_{uniek}",
                Age : age ?? 30,
                Salary : salary ?? 1000m,
                DurationWorked : durationWorked ?? 1,
                Grade : grade ?? "1",
                // RFC sub_adressing: "prefix+suffix@domain" -> handig voor filteren.
                Email : $"{scenarioPrefix}+{uniek}@example.com", 
                ScenarioName : $"{scenarioPrefix} {uniek}"
            );
            
        }

        //---Boundary helpers - gerbuik voor test valideren
        
        ///<summary>Boundary: salary = 0. Heeft Test backend rejected 0 slaris.</summary>
        public static EmployeeData WithZeroSalary(string prefix="boundary-zero-salary")
        => NewUnique(prefix, salary : 0);
        
        ///<summary> Boudary: age = 0, valideer age
        public static EmployeeData WithZeroAge(string prefix="boundary-zero-age")
        => NewUnique(prefix, age:0);

        ///<summary>Boundary: Negatieve salary.</summary>
        public static EmployeeData WithNegativeSalary(string prefix="boundary-neg-salary")
        => NewUnique(prefix, salary:-1000m);

        /// <summary>Boundary: Negative age.</summary>
        public static EmployeeData WithNegativeAge(string prefix = "boundary-neg-age")
            => NewUnique(prefix, age: -1);

    }
}