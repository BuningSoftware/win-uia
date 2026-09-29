using WinUia.Winforms.Tests.Application;

namespace WinUia.Winforms.Tests.Extensions;

public static class AutomationExtensions
{
    extension(Automation)
    {
        public static MainForm LaunchApplication(string path, string? arguments = null)
        {
            var form = Automation.Launch<MainForm>(path, arguments);
            try
            {
                _ = form.MainWindow; // Fail here, not on first use, when the window never appears.
                return form;
            }
            catch
            {
                form.Dispose();
                throw;
            }
        }
    }
}
