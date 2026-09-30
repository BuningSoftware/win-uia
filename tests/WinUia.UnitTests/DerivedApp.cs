namespace WinUia.UnitTests;

/// <summary>A page object that derives from <see cref="App"/>, the way a user's main-window page object would.</summary>
public sealed class DerivedApp : App
{
    public string Title => MainWindow.Name;
}
