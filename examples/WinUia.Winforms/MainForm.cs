namespace WinUia.Winforms;

/// <summary>
/// Fixture window for the WinUia UI tests. The controls are laid out in the designer; WinForms exposes each
/// control's <see cref="Control.Name"/> as its UI Automation AutomationId, so those names are the ids the tests
/// search for. The window is TopMost because the physical-input tests click screen coordinates.
/// </summary>
public partial class MainForm : Form
{
    private int _generation = 1;

    public MainForm()
    {
        InitializeComponent();
    }

    private void btnClick_Click(object? sender, EventArgs e) => lblResult.Text = "Clicked";

    /// <summary>Opens the modal DialogForm and reports how it was closed in lblResult.</summary>
    private void btnOpenDialog_Click(object? sender, EventArgs e)
    {
        using var dialog = new DialogForm();
        var result = dialog.ShowDialog(this);
        lblResult.Text = result == DialogResult.OK ? $"Dialog: OK ({dialog.InputText})" : $"Dialog: {result}";
    }

    /// <summary>
    /// Destroys btnVolatile and creates a new one with the same AutomationId, which makes elements found earlier
    /// stale (their window handle is gone) — the WinUI 3 re-render case.
    /// </summary>
    private void btnRecreate_Click(object? sender, EventArgs e)
    {
        var old = btnVolatile;
        btnVolatile = Recreate(old, $"Volatile {++_generation}");
        Controls.Remove(old);
        old.Dispose();
        Controls.Add(btnVolatile);
    }

    /// <summary>
    /// Recreates the buttons in pnlItems in reverse order: every button gets a new window handle and most change
    /// position, so index-based re-resolution would pick the wrong one.
    /// </summary>
    private void btnReverse_Click(object? sender, EventArgs e)
    {
        var old = pnlItems.Controls.Cast<Button>().ToArray();
        var reversed = old.Reverse().Select(b => Recreate(b, b.Text)).ToArray();

        pnlItems.SuspendLayout();
        foreach (var button in old)
        {
            pnlItems.Controls.Remove(button);
            button.Dispose();
        }

        pnlItems.Controls.AddRange(reversed);
        pnlItems.ResumeLayout();
    }

    private static Button Recreate(Button original, string text) => new()
    {
        Name = original.Name,
        Text = text,
        Location = original.Location,
        Size = original.Size,
        UseVisualStyleBackColor = true,
    };
}
