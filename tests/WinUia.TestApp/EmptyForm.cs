namespace WinUia.TestApp;

/// <summary>The test app's only window: a title and nothing else. Tests of control behaviour use the WinForms example.</summary>
internal sealed class EmptyForm : Form
{
    public EmptyForm()
    {
        Text = "WinUia Test App";
        ClientSize = new Size(300, 150);
    }
}
