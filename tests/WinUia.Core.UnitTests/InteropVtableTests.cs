using WinUia.Core.Elements;

namespace WinUia.Core.UnitTests;

/// <summary>
/// Guards the hand-written vtables: each typed getter must agree with the generic
/// GetCurrentPropertyValue(propertyId). A misaligned slot returns a different property (or crashes).
/// </summary>
public class InteropVtableTests
{
    internal static void AssertTypedGettersMatchPropertyValues(Element element)
    {
        var raw = element.Raw;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(raw.CurrentName ?? "", Is.EqualTo(raw.GetCurrentPropertyValue(30005) ?? ""));                       // Name
            Assert.That(raw.CurrentAutomationId ?? "", Is.EqualTo(raw.GetCurrentPropertyValue(30011) ?? ""));               // AutomationId
            Assert.That(raw.CurrentClassName ?? "", Is.EqualTo(raw.GetCurrentPropertyValue(30012) ?? ""));                  // ClassName
            Assert.That(raw.CurrentFrameworkId ?? "", Is.EqualTo(raw.GetCurrentPropertyValue(30024) ?? ""));                // FrameworkId
            Assert.That(raw.CurrentLocalizedControlType ?? "", Is.EqualTo(raw.GetCurrentPropertyValue(30004) ?? ""));       // LocalizedControlType
            Assert.That(raw.CurrentControlType, Is.EqualTo(raw.GetCurrentPropertyValue(30003)));                            // ControlType
            Assert.That(raw.CurrentProcessId, Is.EqualTo(raw.GetCurrentPropertyValue(30002)));                              // ProcessId
            Assert.That(raw.CurrentIsEnabled, Is.EqualTo(raw.GetCurrentPropertyValue(30010)));                              // IsEnabled
            Assert.That(raw.CurrentIsOffscreen, Is.EqualTo(raw.GetCurrentPropertyValue(30022)));                            // IsOffscreen
            Assert.That(raw.CurrentHasKeyboardFocus, Is.EqualTo(raw.GetCurrentPropertyValue(30008)));                       // HasKeyboardFocus
            Assert.That(raw.CurrentIsKeyboardFocusable, Is.EqualTo(raw.GetCurrentPropertyValue(30009)));                    // IsKeyboardFocusable
            Assert.That(raw.CurrentIsControlElement, Is.EqualTo(raw.GetCurrentPropertyValue(30016)));                       // IsControlElement
            Assert.That(raw.CurrentIsPassword, Is.EqualTo(raw.GetCurrentPropertyValue(30019)));                             // IsPassword
            Assert.That((long)raw.CurrentNativeWindowHandle, Is.EqualTo(Convert.ToInt64(raw.GetCurrentPropertyValue(30020)))); // NativeWindowHandle
            Assert.That(raw.CurrentAcceleratorKey ?? "", Is.EqualTo(raw.GetCurrentPropertyValue(30006) ?? ""));             // AcceleratorKey
            Assert.That(raw.CurrentAccessKey ?? "", Is.EqualTo(raw.GetCurrentPropertyValue(30007) ?? ""));                  // AccessKey
            Assert.That(raw.CurrentHelpText ?? "", Is.EqualTo(raw.GetCurrentPropertyValue(30013) ?? ""));                   // HelpText
            Assert.That(raw.CurrentCulture, Is.EqualTo(raw.GetCurrentPropertyValue(30015)));                                // Culture
            Assert.That(raw.CurrentIsContentElement, Is.EqualTo(raw.GetCurrentPropertyValue(30017)));                       // IsContentElement
            Assert.That(raw.CurrentItemType ?? "", Is.EqualTo(raw.GetCurrentPropertyValue(30021) ?? ""));                   // ItemType
            Assert.That(raw.CurrentOrientation, Is.EqualTo(raw.GetCurrentPropertyValue(30023)));                            // Orientation
            Assert.That(raw.CurrentIsRequiredForForm, Is.EqualTo(raw.GetCurrentPropertyValue(30025)));                      // IsRequiredForForm
            Assert.That(raw.CurrentItemStatus ?? "", Is.EqualTo(raw.GetCurrentPropertyValue(30026) ?? ""));                 // ItemStatus

            var bounds = (double[])raw.GetCurrentPropertyValue(30001)!;                                                      // BoundingRectangle: l, t, w, h
            var rect = raw.CurrentBoundingRectangle;
            Assert.That(rect.left, Is.EqualTo((int)bounds[0]));
            Assert.That(rect.top, Is.EqualTo((int)bounds[1]));
            Assert.That(rect.right, Is.EqualTo((int)(bounds[0] + bounds[2])));
            Assert.That(rect.bottom, Is.EqualTo((int)(bounds[1] + bounds[3])));

            Assert.That(raw.GetRuntimeId(), Is.Not.Empty);
        }
    }

    [Test]
    public void Root_element_typed_getters_match_property_values()
    {
        using var context = new AutomationContext();

        AssertTypedGettersMatchPropertyValues(context.GetRootElement());
    }

    [Test]
    public void Root_element_is_same_as_itself_and_has_children()
    {
        using var context = new AutomationContext();
        var root = context.GetRootElement();

        Assert.That(root.IsSameAs(context.GetRootElement()), Is.True);
        Assert.That(root.FindAll(e => true, TreeScope.Children), Is.Not.Empty);
    }
}
