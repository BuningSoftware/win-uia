namespace WinUia.Core.IntegrationTests;

/// <summary>
/// Elements send physical input through their context's <see cref="AutomationContext.Input"/>, so it can be replaced:
/// these tests click the real desktop element without the real mouse moving.
/// </summary>
public class InputSimulatorTests
{
    [Test]
    public void PhysicalClick_clicks_the_elements_clickable_point_through_the_contexts_input()
    {
        var input = new RecordingInputSimulator();
        using var context = new AutomationContext();
        context.Input = input;
        var root = context.GetRootElement();
        var point = root.GetClickablePoint();

        root.PhysicalClick();

        Assert.That(input.Sent, Is.EqualTo(new[] { $"ClickAt({point.X}, {point.Y}, Left)" }));
    }

    [Test]
    public void With_ShowPointer_PhysicalClick_first_moves_the_pointer_over_PointerMoveDuration()
    {
        var input = new RecordingInputSimulator();
        using var context = new AutomationContext();
        context.Input = input;
        context.ShowPointer = true;
        context.PointerMoveDuration = TimeSpan.FromMilliseconds(250);
        var root = context.GetRootElement();
        var point = root.GetClickablePoint();

        root.PhysicalClick();

        Assert.That(input.Sent, Is.EqualTo(new[]
        {
            $"MoveTo({point.X}, {point.Y}, 250 ms)",
            $"ClickAt({point.X}, {point.Y}, Left)",
        }));
    }
}
