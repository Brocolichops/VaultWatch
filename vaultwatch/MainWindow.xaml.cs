using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace VaultWatch;

public partial class MainWindow : Window
{
    private readonly MessageBus messageBus = new MessageBus();
    private readonly List<ISensor> sensors;
    private readonly string currentUser;

    public ObservableCollection<DeviceMessage> Messages { get; } = new ObservableCollection<DeviceMessage>();

    public ObservableCollection<DoorLock> DoorLocks { get; } = new ObservableCollection<DoorLock>
    {
        new DoorLock("door-vault-01", "BANK VAULT 01"),
        new DoorLock("door-vault-02", "BANK VAULT 02"),
        new DoorLock("door-vault-03", "BANK VAULT 03"),
        new DoorLock("door-front", "FRONT ENTRANCE"),
        new DoorLock("door-loading", "LOADING ENTRANCE")
    };

    public MainWindow(string currentUser)
    {
        InitializeComponent();
        this.currentUser = currentUser;
        DataContext = this;

        messageBus.Subscribe(AddMessage);
        sensors = new List<ISensor>
        {
            new MotionSensor(messageBus, "rtu-02"),
            new TemperatureSensor(messageBus, "rtu-03")
        };

        GenerateSensorMessages();
    }

    private void SendMessages_Click(object sender, RoutedEventArgs e)
    {
        GenerateSensorMessages();
    }

    private void LockDoor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is DoorLock doorLock)
        {
            CommandResult result = doorLock.Lock();
            ReportCommand(new CommandRecord(doorLock.DeviceId, "Lock", currentUser, result));
        }
    }

    private void UnlockDoor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is DoorLock doorLock)
        {
            MessageBoxResult answer = MessageBox.Show(
               this,
               $"Unlock {doorLock.Name}?",
               "Confirm unlock",
               MessageBoxButton.YesNo,
               MessageBoxImage.Warning);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            CommandResult result = doorLock.Unlock();
            ReportCommand(new CommandRecord(doorLock.DeviceId, "Unlock", currentUser, result));
        }
    }

    private void InitiateLockdown_Click(object sender, RoutedEventArgs e)
    {
        LockdownAuthenticationWindow authenticationWindow = new LockdownAuthenticationWindow
        {
            Owner = this
        };

        if (authenticationWindow.ShowDialog() != true)
        {
            return;
        }

        foreach (DoorLock doorLock in DoorLocks)
        {
            doorLock.ActivateLockdown();
        }

        LockdownStatusText.Text = "LOCKDOWN ACTIVE // ALL DOORS LOCKED // SILENT SOS SIMULATED";
        LockdownBanner.Visibility = Visibility.Visible;
    }

    private void GenerateSensorMessages()
    {
        foreach (ISensor sensor in sensors)
        {
            sensor.SendMessage();
        }
    }

    private void AddMessage(DeviceMessage message)
    {
        Messages.Add(message);
    }

    private void ReportCommand(CommandRecord command)
    {
        // Show the command result in the message grid (FUN-015)
        messageBus.Publish(new DeviceMessage(
            command.DeviceId,
            "DoorLock",
            $"Command: {command.Action}",
            $"{command.Result} (by {command.IssuedBy})"));
    }
}