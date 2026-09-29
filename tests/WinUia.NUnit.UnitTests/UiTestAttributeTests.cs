namespace WinUia.NUnit.UnitTests;

[UiTest]
public class UiTestAttributeTests
{
    private bool _desktopHeldDuringSetUp;

    /// <summary>True when another thread cannot take the desktop right now.</summary>
    private static bool DesktopIsHeld()
    {
        var held = false;
        var probe = new Thread(() =>
        {
            try
            {
                using var _ = DesktopLock.Acquire(TimeSpan.FromMilliseconds(200));
            }
            catch (TimeoutException)
            {
                held = true;
            }
        });
        probe.Start();
        probe.Join();
        return held;
    }

    [SetUp]
    public void SetUp() => _desktopHeldDuringSetUp = DesktopIsHeld();

    [Test]
    public void The_desktop_is_held_during_SetUp_and_the_test()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_desktopHeldDuringSetUp, Is.True, "the lock is taken before [SetUp] runs");
            Assert.That(DesktopIsHeld(), Is.True, "the lock is held while the test runs");
        }
    }

    [Test]
    [UiTest(TimeoutSeconds = 5)]
    public void UiTest_on_both_the_class_and_the_method_does_not_deadlock()
    {
        Assert.That(DesktopIsHeld(), Is.True);
    }

    [Test]
    [UiTest]
    public void UiTest_puts_the_test_in_the_UI_category()
    {
        Assert.That(TestContext.CurrentContext.Test.Properties["Category"], Does.Contain(UiTestAttribute.Category));
    }
}
