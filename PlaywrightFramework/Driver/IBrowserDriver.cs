namespace PlaywrightFramework.Driver
{
    public interface IBrowserDriver
    {
        // --- Navigatie ---
        Task NavigateAsync(string url);
        Task<string > GetTitleAsync();
        Task<string> GetCurrentUrlAsync();

        // --- Acties ---
        Task ClickAsync(string selector);
        Task FillAsync(string selector, string value);
        //Task SelectOptionAsync(string selector, string valueOrLabel);
        Task SelectByLabelAsync(string selector, string label);
        Task SelectByValueAsync(string selector, string value);
        Task<IReadOnlyList<string>> GetSelectOptionsAsync(string selector);
        //Task PressKeyAsync(string selector, string key);
        

        // --- Queries ---
        Task<bool> IsVisibleAsync(string selector);
        Task<bool> IsVisibleAsync(string selector, int timeoutMs);
        Task<string> GetTextAsync(string selector);
        Task<int> GetCountAsync(string selector);

        // --- Wachten ---
        Task WaitForVisibleAsync(string selector, int? timeoutMs = null);

        // --- Lifecycle ---
        Task DisposeAsync();

    }
    
}