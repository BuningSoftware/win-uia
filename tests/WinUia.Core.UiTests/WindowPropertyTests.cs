using WinUia.Testing.Shared;

namespace WinUia.Core.UiTests;

/// <summary>
/// A real application window reports the same values through Element's typed properties and through the generic
/// GetPropertyValue, so the hand-written vtable slots behind them line up for windows too (the desktop root is
/// covered by the integration tests).
/// </summary>
public class WindowPropertyTests
{
    [Test]
    public void A_window_reports_the_same_values_through_typed_properties_and_GetPropertyValue()
    {
        using var app = App.Launch(AppPaths.TestApp);
        var window = app.MainWindow;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(window.Name, Is.EqualTo(window.GetPropertyValue(30005) ?? ""));                  // Name
            Assert.That(window.AutomationId, Is.EqualTo(window.GetPropertyValue(30011) ?? ""));          // AutomationId
            Assert.That(window.ClassName, Is.EqualTo(window.GetPropertyValue(30012) ?? ""));             // ClassName
            Assert.That(window.FrameworkId, Is.EqualTo(window.GetPropertyValue(30024) ?? ""));           // FrameworkId
            Assert.That((int)window.ControlType, Is.EqualTo(window.GetPropertyValue(30003)));            // ControlType
            Assert.That(window.ProcessId, Is.EqualTo(window.GetPropertyValue(30002)));                   // ProcessId
            Assert.That(window.IsEnabled, Is.EqualTo(window.GetPropertyValue(30010)));                   // IsEnabled
            Assert.That(window.IsOffscreen, Is.EqualTo(window.GetPropertyValue(30022)));                 // IsOffscreen
            Assert.That((long)window.NativeWindowHandle, Is.EqualTo(Convert.ToInt64(window.GetPropertyValue(30020)))); // NativeWindowHandle

            var bounds = (double[])window.GetPropertyValue(30001)!;                                     // BoundingRectangle: l, t, w, h
            var rect = window.BoundingRectangle;
            Assert.That(rect.Left, Is.EqualTo((int)bounds[0]));
            Assert.That(rect.Top, Is.EqualTo((int)bounds[1]));
            Assert.That(rect.Right, Is.EqualTo((int)(bounds[0] + bounds[2])));
            Assert.That(rect.Bottom, Is.EqualTo((int)(bounds[1] + bounds[3])));
        }
    }
}
