using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Windows;

namespace VaultWatch;

public partial class LoginWindow : Window
{
    private const int MaximumFailedAttempts = 4;
    private const string CredentialFileName = "credentials.csv";

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

        if (!TryAuthenticateFromCsv(
                UsernameTextBox.Text.Trim(),
                PasswordBox.Password,
                "User",
                out bool authenticated,
                out string? databaseError))
        {
            LoginStatusText.Text = databaseError;
            return;
        }

        if (authenticated)
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
        if (!TryAuthenticateFromCsv(
                AdminUsernameTextBox.Text.Trim(),
                AdminPasswordBox.Password,
                "Admin",
                out bool administratorAuthenticated,
                out string? databaseError))
        {
            AdminStatusText.Text = databaseError;
            return;
        }

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

    private static bool TryAuthenticateFromCsv(
        string username,
        string password,
        string requiredRole,
        out bool authenticated,
        out string? error)
    {
        authenticated = false;
        error = null;
        string credentialFilePath = Path.Combine(AppContext.BaseDirectory, CredentialFileName);

        if (!File.Exists(credentialFilePath))
        {
            error = $"Credential database not found: {CredentialFileName} beside the application.";
            return false;
        }

        try
        {
            using TextFieldParser parser = new TextFieldParser(credentialFilePath)
            {
                TextFieldType = FieldType.Delimited,
                HasFieldsEnclosedInQuotes = true
            };
            parser.SetDelimiters(",");

            if (parser.EndOfData)
            {
                error = "Credential database is empty; expected Username,Password,Role columns.";
                return false;
            }

            string[]? headers = parser.ReadFields();
            if (headers is null
                || headers.Length != 3
                || !string.Equals(headers[0].Trim(), "Username", StringComparison.OrdinalIgnoreCase)
                || !string.Equals(headers[1].Trim(), "Password", StringComparison.OrdinalIgnoreCase)
                || !string.Equals(headers[2].Trim(), "Role", StringComparison.OrdinalIgnoreCase))
            {
                error = "Credential database must start with the columns Username,Password,Role.";
                return false;
            }

            HashSet<string> usernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (!parser.EndOfData)
            {
                string[]? fields = parser.ReadFields();
                if (fields is null || (fields.Length == 1 && string.IsNullOrWhiteSpace(fields[0])))
                {
                    continue;
                }

                if (fields.Length != 3
                    || string.IsNullOrWhiteSpace(fields[0])
                    || string.IsNullOrEmpty(fields[1])
                    || string.IsNullOrWhiteSpace(fields[2]))
                {
                    error = "Credential database contains an invalid record; each row needs Username,Password,Role.";
                    return false;
                }

                string recordUsername = fields[0].Trim();
                if (!usernames.Add(recordUsername))
                {
                    error = $"Credential database contains a duplicate username: {recordUsername}.";
                    return false;
                }

                if (string.Equals(recordUsername, username, StringComparison.OrdinalIgnoreCase)
                    && fields[1] == password
                    && string.Equals(fields[2].Trim(), requiredRole, StringComparison.OrdinalIgnoreCase))
                {
                    authenticated = true;
                }
            }

            return true;
        }
        catch (MalformedLineException ex)
        {
            error = $"Credential database contains malformed CSV at line {ex.LineNumber}.";
            return false;
        }
        catch (IOException ex)
        {
            error = $"Could not read the credential database: {ex.Message}";
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            error = $"Could not read the credential database: {ex.Message}";
            return false;
        }
    }

    private void SetLockedOutState()
    {
        UsernameTextBox.IsEnabled = false;
        PasswordBox.IsEnabled = false;
        LoginButton.IsEnabled = false;
        AdminApprovalPanel.Visibility = Visibility.Visible;
    }
}
