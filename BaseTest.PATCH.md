# Patch huong dan: cap nhat BaseTest.cs

Day la diff de them retry-logging vao `BaseTest.cs` hien tai cua ban.
Ban tu apply patch nay vao file goc trong repo - khong ghi de.

## File: `PlaywrightTests/Tests/BaseTest.cs`

### Doi 2 dong:

**Dong 16** (declaration):
```diff
- public abstract class BaseTest
+ public abstract partial class BaseTest
```
> Ly do: cho phep file `BaseTest.RetryHooks.cs` add method vao cung class.

### Dong 56-63 (TearDown), them call `LogRetryStatus()`:

**Truoc:**
```csharp
[TearDown]
public async Task TearDown()
{
    if(PlaywrightDriver is not null)
    {
        await PlaywrightDriver.DisposeAsync();
    }
}
```

**Sau:**
```csharp
[TearDown]
public async Task TearDown()
{
    // Log status retry truoc khi dispose driver - de TestContext.WriteLine
    // van active. (Sau Dispose se khong log duoc.)
    LogRetryStatus();

    if (PlaywrightDriver is not null)
    {
        await PlaywrightDriver.DisposeAsync();
    }
}
```

> `LogRetryStatus()` la method partial trong `BaseTest.RetryHooks.cs`.
> Neu khong them keyword `partial`, file moi se khong compile.

## Sau khi apply:

```bash
dotnet build PlaywrightTests/PlaywrightTests.csproj
# Phai pass khong loi.
```

Test thu retry: tao 1 test gia flaky de xac nhan:

```csharp
[Test]
[Category(TestCategories.Flaky)]
[Retry(2)]
public void Flaky_demo_should_eventually_pass()
{
    // Random fail 50% - retry 2 lan se gan nhu chac pass.
    if (Random.Shared.Next(2) == 0)
        Assert.Fail("simulated flake");
}
```

Chay 5 lan - phai thay log `[RETRY-RESULT] PASS` xen ke `FAIL`.
