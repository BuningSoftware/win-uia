using System.Diagnostics;

namespace WinUia.Input.UiTests;

/// <summary>Moves the real cursor, so these run as UI tests (one desktop, interactive session).</summary>
public class CursorTests
{
    private (int X, int Y) _original;

    [SetUp]
    public void RememberCursor() => _original = Win32InputSimulator.GetCursorPosition();

    [TearDown]
    public void RestoreCursor() => Win32InputSimulator.MoveTo(_original.X, _original.Y);

    [Test]
    public void MoveTo_puts_the_cursor_where_GetCursorPosition_reports_it()
    {
        Win32InputSimulator.MoveTo(210, 160);

        Assert.That(Win32InputSimulator.GetCursorPosition(), Is.EqualTo((210, 160)));
    }

    [Test]
    public void A_smooth_move_ends_exactly_on_the_target_after_about_its_duration()
    {
        Win32InputSimulator.MoveTo(200, 200);
        var stopwatch = Stopwatch.StartNew();

        Win32InputSimulator.MoveTo(500, 350, TimeSpan.FromMilliseconds(300));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Win32InputSimulator.GetCursorPosition(), Is.EqualTo((500, 350)));
            Assert.That(stopwatch.ElapsedMilliseconds, Is.InRange(250, 3000));
        }
    }

    [Test]
    public void A_smooth_move_to_where_the_cursor_already_is_returns_at_once()
    {
        Win32InputSimulator.MoveTo(300, 250);
        var stopwatch = Stopwatch.StartNew();

        Win32InputSimulator.MoveTo(300, 250, TimeSpan.FromSeconds(2));

        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(500));
    }

    [Test]
    public void MoveTo_a_point_on_no_screen_throws_instead_of_leaving_the_cursor_elsewhere()
    {
        // Windows clamps the cursor to the screens, so it can never arrive at a point beyond all of them.
        var ex = Assert.Throws<InvalidOperationException>(() => Win32InputSimulator.MoveTo(100_000, 100_000));

        Assert.That(ex.Message, Does.Contain("(100000, 100000)"));
    }
}
