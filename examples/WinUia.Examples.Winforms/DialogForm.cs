namespace WinUia.Examples.Winforms;

/// <summary>Modal dialog opened by btnOpenDialog: asks for a value, closed with OK or Cancel.</summary>
public partial class DialogForm : Form
{
    public DialogForm()
    {
        InitializeComponent();
    }

    /// <summary>The value typed into the dialog.</summary>
    public string InputText => txtDialogInput.Text;
}
