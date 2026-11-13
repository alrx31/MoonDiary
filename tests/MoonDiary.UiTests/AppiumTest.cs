using System.Runtime.CompilerServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Interactions;
using Xunit.Abstractions;

namespace MoonDiary.UiTests;

public abstract class AppiumTest : IDisposable
{
    public const string AppPackage = "com.moondiary.app";

    private readonly ITestOutputHelper output;
    private readonly string testResultsDir;
    private readonly string apkPath;
    private readonly Uri appiumUrl;
    private readonly string deviceName;
    private AndroidDriver? driver;

    protected AppiumTest(ITestOutputHelper output)
    {
        this.output = output;
        apkPath = Path.GetFullPath(GetRequiredEnvironmentVariable("MOONDIARY_APK_PATH"));

        if (!File.Exists(apkPath))
        {
            throw new FileNotFoundException($"APK не найден по пути из MOONDIARY_APK_PATH: '{apkPath}'.", apkPath);
        }

        appiumUrl = new Uri(GetEnvironmentVariableOrDefault("MOONDIARY_APPIUM_URL", "http://127.0.0.1:4723"));
        deviceName = GetEnvironmentVariableOrDefault("MOONDIARY_DEVICE_NAME", "Android");
        testResultsDir = Path.GetFullPath(
            GetEnvironmentVariableOrDefault("MOONDIARY_TEST_RESULTS_DIR", "TestResults/Appium"));
    }

    protected AndroidDriver Driver => driver ?? throw new InvalidOperationException("Приложение не запущено.");

    public void Dispose()
    {
        driver?.Quit();
        GC.SuppressFinalize(this);
    }

    protected void Launch(string localNow, string seed = "empty")
    {
        var database = $"ui-{Guid.NewGuid():N}.db";
        var options = new AppiumOptions
        {
            PlatformName = "Android",
            AutomationName = "UIAutomator2",
            DeviceName = deviceName,
            App = apkPath
        };

        options.AddAdditionalAppiumOption("appPackage", AppPackage);
        options.AddAdditionalAppiumOption("appActivity", $"{AppPackage}.MainActivity");
        options.AddAdditionalAppiumOption("autoGrantPermissions", true);
        options.AddAdditionalAppiumOption("autoLaunch", false);

        driver = new AndroidDriver(appiumUrl, options);
        driver.ExecuteScript("mobile: startActivity", new Dictionary<string, object>
        {
            ["component"] = $"{AppPackage}/{AppPackage}.MainActivity",
            ["stop"] = true,
            ["wait"] = true,
            ["extras"] = new[]
            {
                new[] { "s", "moondiary_now", localNow },
                new[] { "s", "moondiary_db", database },
                new[] { "s", "moondiary_seed", seed }
            }
        });
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(15);
    }

    protected void RunScenario(Action scenario, [CallerMemberName] string testName = "")
    {
        try
        {
            scenario();
        }
        catch
        {
            SaveScreenshot(testName);

            throw;
        }
    }

    protected AppiumElement Find(string automationId) => Driver.FindElement(MobileBy.Id(automationId));

    protected void Tap(string automationId) => Find(automationId).Click();

    protected void Slide(string automationId, double share)
    {
        var element = Find(automationId);
        var y = element.Location.Y + (element.Size.Height / 2);
        var from = element.Location.X + (element.Size.Height / 2);
        var to = from + (int)((element.Size.Width - element.Size.Height) * share);
        var finger = new PointerInputDevice(PointerKind.Touch);
        var sequence = new ActionSequence(finger);
        const int steps = 12;

        sequence.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, from, y, TimeSpan.Zero));
        sequence.AddAction(finger.CreatePointerDown(MouseButton.Left));

        for (var step = 1; step <= steps; step++)
        {
            sequence.AddAction(finger.CreatePointerMove(
                CoordinateOrigin.Viewport,
                from + ((to - from) * step / steps),
                y,
                TimeSpan.FromMilliseconds(40)));
        }

        sequence.AddAction(finger.CreatePause(TimeSpan.FromMilliseconds(150)));
        sequence.AddAction(finger.CreatePointerUp(MouseButton.Left));
        Driver.PerformActions([sequence]);
    }

    protected bool Exists(string automationId)
    {
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);

        try
        {
            return Driver.FindElements(MobileBy.Id(automationId)).Count > 0;
        }
        finally
        {
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(15);
        }
    }

    private void SaveScreenshot(string testName)
    {
        if (driver is null)
        {
            return;
        }

        Directory.CreateDirectory(testResultsDir);

        var filePath = Path.Combine(testResultsDir, $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");

        driver.GetScreenshot().SaveAsFile(filePath);
        output.WriteLine($"Скриншот сохранён: {filePath}");
    }

    private static string GetRequiredEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);

        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"Переменная окружения '{name}' не задана.")
            : value;
    }

    private static string GetEnvironmentVariableOrDefault(string name, string defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(name);

        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }
}
