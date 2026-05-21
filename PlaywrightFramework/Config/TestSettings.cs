using System;
using System.Collections.Generic;
using System.Linq;

namespace PlaywrightFramework.Config
{
    
    public class TestSettings
    {
        /// <summary>
        /// TestSettings is an object that contains all the configuration needed to run tests: browser, context, and app configurations. 
        /// It groups these settings together into a single "test configuration."
        /// </summary>
        public BrowserSettings Browser { get; set; } = new();
        public ContextSettings Context { get; set; } = new();
        public AppSettings App { get; set; } = new();

        public void Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(App.BaseUrl))
                errors.Add("App.BaseUrl is required.");

            if (string.IsNullOrWhiteSpace(App.Username))
                errors.Add("App.Username is required.");

            if (string.IsNullOrWhiteSpace(App.Password))
                errors.Add(
                    "App.Password is required. " +
                    "Set it with: dotnet user-secrets set \"App:Password\" \"...\" " +
                    "or use environment variable App__Password.");

            if (App.Timeout <= 0)
                errors.Add("App.Timeout must be greater than 0.");

            if (Context.ViewportWidth <= 0)
                errors.Add("Context.ViewportWidth must be greater than 0.");

            if (Context.ViewportHeight <= 0)
                errors.Add("Context.ViewportHeight must be greater than 0.");

            if (string.IsNullOrWhiteSpace(Context.Locale))
                errors.Add("Context.Locale is required.");

            if (!Enum.IsDefined(typeof(DriverType), Browser.DriverType))
                errors.Add("Browser.DriverType is invalid.");

            if (Browser.SlowMo < 0)
                errors.Add("Browser.SlowMo must be greater than or equal to 0.");

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Invalid test configuration:" + Environment.NewLine +
                    string.Join(Environment.NewLine, errors.Select(e => "- " + e)));
            }
        }

        public TestSettings Clone()
        {
            return new TestSettings
            {
                App = App.Clone(),
                Browser = Browser.Clone(),
                Context = Context.Clone()
            };
        }
        
    }
    
    // Config/BrowserSettings.cs
    public class BrowserSettings
    {
        public bool Headless { get; set; }
        public int SlowMo { get; set; }
        public string Channel { get; set; } = string.Empty;
        public DriverType DriverType { get; set; } = DriverType.Chromium;
        public BrowserSettings Clone()
        {
            return new BrowserSettings
            {
                DriverType = DriverType,
                Headless = Headless,
                SlowMo = SlowMo,
                Channel = Channel
            };
        }
    }

    // Config/ContextSettings.cs
    public class ContextSettings
    {
        public int ViewportWidth { get; set; } = 1920;
        public int ViewportHeight { get; set; } = 1080;
        public string Locale { get; set; } = "en-US";
        public string? RecordVideoDir { get; set; }
        public bool IgnoreHttpsErrors { get; set; }
        public string? AuthStatePath { get; set; }
        public ContextSettings Clone()
        {
            return new ContextSettings
            {
                ViewportWidth = ViewportWidth,
                ViewportHeight = ViewportHeight,
                Locale = Locale,
                IgnoreHttpsErrors = IgnoreHttpsErrors,
                RecordVideoDir = RecordVideoDir,
                AuthStatePath = AuthStatePath
            };
        }
    }

    // Config/AppSettings.cs
    public class AppSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Timeout { get; set; } = 30000;
        public AppSettings Clone()
        {
            return new AppSettings
            {
                BaseUrl = BaseUrl,
                Username = Username,
                Password = Password,
                Timeout = Timeout
            };
        }
    }
    
}