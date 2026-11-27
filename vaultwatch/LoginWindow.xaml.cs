using System.IO;
using System.Windows;

namespace VaultWatch;

public partial class LoginWindow : Window
{
    private const string ClientUsername = "user";
    private const string ClientPassword = "0000";
    private const string AdministratorUsername = "admin";
    private const string AdministratorPassword = "2468";
    private const int MaximumFailedAttempts = 4;

    private static readonly string LockoutFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "VaultWatch",
        "login.locked");

    private int failedAttempts;
    private bool isLockedOut;

    public LoginWindow()
    {
        InitializeComponent();

        if (File.Exists(LockoutFilePath))
        {
            isLockedOut = true;
            SetLockedOutState();
            LoginStatusText.Text = "Sign-in remains disabled. Administrator approval is required.";
            AdminUsernameTextBox.Focus();
        }
        else
        {
            UsernameTextBox.Focus();
        }
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        if (isLockedOut)
        {
            return;
        }

        if (string.Equals(UsernameTextBox.Text.Trim(), ClientUsername, StringComparison.OrdinalIgnoreCase)
            && PasswordBox.Password == ClientPassword)
        {
            failedAttempts = 0;
            DialogResult = true;
            return;
        }

        failedAttempts++;
        PasswordBox.Clear();

        if (failedAttempts >= MaximumFailedAttempts)
        {
            isLockedOut = true;
            SetLockedOutState();

            try
            {
                string? lockoutDirectory = Path.GetDirectoryName(LockoutFilePath);
                if (lockoutDirectory is null)
                {
                    throw new IOException("The lockout storage path is invalid.");
                }

                Directory.CreateDirectory(lockoutDirectory);
                File.WriteAllText(LockoutFilePath, string.Empty);
                LoginStatusText.Text = "Sign-in disabled. An administrator must approve access.";
            }
            catch (IOException ex)
            {
                LoginStatusText.Text = $"Sign-in is locked for this session, but the lockout could not be saved: {ex.Message}";
            }
            catch (UnauthorizedAccessException ex)
            {
                LoginStatusText.Text = $"Sign-in is locked for this session, but the lockout could not be saved: {ex.Message}";
            }

            return;
        }

        int attemptsRemaining = MaximumFailedAttempts - failedAttempts;
        LoginStatusText.Text = $"Invalid username or password. {attemptsRemaining} attempt(s) remaining.";
    }

    private void ApproveUnlock_Click(object sender, RoutedEventArgs e)
    {
        bool administratorAuthenticated =
            string.Equals(AdminUsernameTextBox.Text.Trim(), AdministratorUsername, StringComparison.OrdinalIgnoreCase)
            && AdminPasswordBox.Password == AdministratorPassword;

        if (!administratorAuthenticated)
        {
            AdminStatusText.Text = "Administrator credentials were not accepted.";
            AdminPasswordBox.Clear();
            return;
        }

        try
        {
            File.Delete(LockoutFilePath);
        }
        catch (IOException ex)
        {
            AdminStatusText.Text = $"Could not clear the saved lockout: {ex.Message}";
            return;
        }
        catch (UnauthorizedAccessException ex)
        {
            AdminStatusText.Text = $"Could not clear the saved lockout: {ex.Message}";
            return;
        }

        failedAttempts = 0;
        isLockedOut = false;
        UsernameTextBox.IsEnabled = true;
        PasswordBox.IsEnabled = true;
        LoginButton.IsEnabled = true;
        AdminApprovalPanel.Visibility = Visibility.Collapsed;
        AdminUsernameTextBox.Clear();
        AdminPasswordBox.Clear();
        AdminStatusText.Text = string.Empty;
        LoginStatusText.Text = "Access re-enabled by administrator. Sign in to continue.";
        UsernameTextBox.Focus();
    }

    private void SetLockedOutState()
    {
        UsernameTextBox.IsEnabled = false;
        PasswordBox.IsEnabled = false;
        LoginButton.IsEnabled = false;
        AdminApprovalPanel.Visibility = Visibility.Visible;
    }
}
