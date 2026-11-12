using System.Runtime.CompilerServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using Xunit.Abstractions;

namespace MoonDiary.UiTests;

public abstract class AppiumTest : IDisposable
{
    private const string AppPackage = "com.moondiary.app";
    private const string AppActivity = "com.moondiary.app.MainActivity";

    private readonly ITestOutputHelper _output;
    private readonly string _testResultsDir;

    protected AndroidDriver Driver { get; }

    protected AppiumTest(ITestOutputHelper output)
    {
        _output = output;

        var apkPath = GetRequiredEnvironmentVariable("MOONDIARY_APK_PATH");
        apkPath = Path.GetFullPath(apkPath);
        ValidateApkPath(apkPath);

        var appiumUrl = GetEnvironmentVariableOrDefault("MOONDIARY_APPIUM_URL", "http://127.0.0.1:4723");
        var deviceName = GetEnvironmentVariableOrDefault("MOONDIARY_DEVICE_NAME", "Android");
        _testResultsDir = GetEnvironmentVariableOrDefault("MOONDIARY_TEST_RESULTS_DIR", "TestResults/Appium");
        _testResultsDir = Path.GetFullPath(_testResultsDir);

        var options = new AppiumOptions();
        options.PlatformName = "Android";
        options.AutomationName = "UIAutomator2";
        options.DeviceName = deviceName;
        options.App = apkPath;
        options.AddAdditionalAppiumOption("appPackage", AppPackage);
        options.AddAdditionalAppiumOption("appActivity", AppActivity);
        options.AddAdditionalAppiumOption("fullReset", true);
        options.AddAdditionalAppiumOption("autoGrantPermissions", true);

        Driver = new AndroidDriver(new Uri(appiumUrl), options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(15);
    }

    public void Dispose()
    {
        Driver.Quit();
        GC.SuppressFinalize(this);
    }

    protected AppiumElement FindElement(string automationId)
    {
        return Driver.FindElement(MobileBy.Id(automationId));
    }

    protected void SaveScreenshot([CallerMemberName] string testName = "")
    {
        Directory.CreateDirectory(_testResultsDir);
        var fileName = $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        var filePath = Path.Combine(_testResultsDir, fileName);
        var screenshot = Driver.GetScreenshot();
        screenshot.SaveAsFile(filePath);
        _output.WriteLine($"Screenshot saved: {filePath}");
    }

    private static string GetRequiredEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Environment variable '{name}' is not set.");
        }
        return value;
    }

    private static string GetEnvironmentVariableOrDefault(string name, string defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    private static void ValidateApkPath(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"APK file not found at path specified by MOONDIARY_APK_PATH: '{path}'.", path);
        }
    }
}
