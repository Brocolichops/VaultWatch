using System.Windows;

namespace VaultWatch;

public partial class LockdownAuthenticationWindow : Window
{
    private const string RequiredPhrase = "INITIATE LOCKDOWN";
    private const string DemoPin = "2468";

    public LockdownAuthenticationWindow()
    {
        InitializeComponent();
    }

    private void Authorize_Click(object sender, RoutedEventArgs e)
    {
        if (!string.Equals(ConfirmationTextBox.Text.Trim(), RequiredPhrase, StringComparison.OrdinalIgnoreCase))
        {
            ErrorTextBlock.Text = "Confirmation phrase did not match.";
            return;
        }

        if (PinBox.Password != DemoPin)
        {
            ErrorTextBlock.Text = "Invalid security PIN.";
            PinBox.Clear();
            return;
        }

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
