namespace WinUia.TestApp;

internal static class Program
{
    /// <summary>Shows <see cref="EmptyForm"/>. With <c>--ignore-close</c> the window refuses to close, so it has to be killed.</summary>
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        var form = new EmptyForm();

        if (args.Contains("--ignore-close"))
            form.FormClosing += (_, e) => e.Cancel = true;

        Application.Run(form);
    }
}
