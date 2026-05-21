using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.VisualBasic;
using NUnit.Framework;

namespace PlaywrightTests.TestData
{
    public record InvalidLoginCase(
        string Username, string Password,
        string? ExpectedUserNameError,
        string? ExpectedPasswordError,
        string? ExpectedSummaryError,
        string Scenario)
    {
        public override string ToString() => Scenario;
    }
    public static class InvalidLoginCaseSource
    {
        // QUAN TRỌNG: dùng TestContext.CurrentContext.TestDirectory
        // thay vì đường dẫn tuyệt đối → portable, chạy được trên CI
        public static IEnumerable<InvalidLoginCase> Case()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "invalid-logins.json");

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<InvalidLoginCase>>(json, 
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive =true
            })!;
        }        

    }
}