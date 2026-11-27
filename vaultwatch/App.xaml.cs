using System.Windows;

namespace VaultWatch;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        LoginWindow loginWindow = new LoginWindow();
        if (loginWindow.ShowDialog() != true)
        {
            Shutdown();
            return;
        }

        MainWindow mainWindow = new MainWindow(loginWindow.LoggedInUsername);
        MainWindow = mainWindow;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        mainWindow.Show();
    }
}
