using Xunit;
using Xunit.Abstractions;

namespace MoonDiary.UiTests;

public class MainPageTests : AppiumTest
{
    public MainPageTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void CounterButton_IncrementsOnClick()
    {
        try
        {
            var helloLabel = FindElement("HelloLabel");
            Assert.True(helloLabel.Displayed, "HelloLabel is not displayed.");

            var counterBtn = FindElement("CounterBtn");
            Assert.Equal("Click me", counterBtn.Text);

            counterBtn.Click();

            counterBtn = FindElement("CounterBtn");
            Assert.Equal("Clicked 1 time", counterBtn.Text);
        }
        catch
        {
            SaveScreenshot();
            throw;
        }
    }
}
