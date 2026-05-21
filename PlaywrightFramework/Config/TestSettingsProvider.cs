using System;
using Microsoft.Extensions.Configuration;

namespace PlaywrightFramework.Config
{
    public static class TestSettingsProvider
    {
        private static readonly Lazy<TestSettings> _settings = new(Load);

        public static TestSettings Get() => _settings.Value.Clone();

        private static TestSettings Load()
        {
            var env =
                Environment.GetEnvironmentVariable("TEST_ENVIRONMENT")
                ?? "Development";

            var envFile = $"appsettings.{env}.json";

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("AppSettings/appsettings.json", optional: false)
                .AddJsonFile($"AppSettings/{envFile}", optional: true)
                .AddUserSecrets<TestSettings>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            Console.WriteLine(
                $"[Config] env={env} configuredChain=[appsettings.json, {envFile}, UserSecrets, EnvVars]");

            var settings = new TestSettings();
            config.Bind(settings);

            return settings;
        }
       
        
    }
}