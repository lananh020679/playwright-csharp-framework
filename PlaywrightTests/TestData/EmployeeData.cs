using NUnit.Framework.Internal;

namespace PlaywrightTests.TestData
{
     /// <summary>
    /// DTO mô tả 1 bộ dữ liệu cho EmployeeFormPage.FillAsync.
    /// Override ToString() để NUnit Test Explorer hiển thị tên case dễ đọc.
    /// </summary>
    public record EmployeeData(
        string Name,
        int Age,
        decimal Salary,
        int DurationWorked,
        string Grade,
        string Email,
        string ScenarioName
    )
    {
        public override string ToString()
        {
            return ScenarioName;
        }
    }

    public static class EmployeeTestData
    {
        public static IEnumerable<EmployeeData> ValidEmployees()
        {
            var suffix=Guid.NewGuid().ToString("N")[..8];
            yield return new EmployeeData(
                Name: "Alice Nguyen", Age : 24, Salary: 1500m, DurationWorked: 2,
                Grade: "1", Email: $"alice+{suffix}@example.com",
                ScenarioName: "Standard A-grade junior"
                );
            yield return new EmployeeData(
                Name: "Bob Tran", Age : 35,   Salary: 9999.99m, DurationWorked: 10,
                Grade: "2", Email: $"bob+{suffix}@example.com",
                ScenarioName: "Senior with decimal salary"
                );
            yield return new EmployeeData(
                Name: "Châu Đỗ",  Age : 45,   Salary: 1m, DurationWorked: 0,
                Grade: "3", Email: $"chau+{suffix}test@example.com",
                ScenarioName: "Edge: unicode name, plus-email, min salary"
                );            
        }
    }
}