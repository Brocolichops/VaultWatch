using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace VaultWatch;

public partial class MainWindow : Window
{
    private readonly MessageBus messageBus = new MessageBus();
    private readonly List<ReplayDevice> replayDevices;
    private readonly string currentUser;

    public ObservableCollection<DeviceMessageLog> DeviceLogs { get; } = new ObservableCollection<DeviceMessageLog>();

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
        replayDevices = new List<ReplayDevice>
        {
            new ReplayDevice(messageBus, "MOT-BACKHALL-01", "MotionSensor", FileDataSource.FromDataFolder("motion_backhall.csv")),
            new ReplayDevice(messageBus, "THM-BACKHALL-01", "ThermalSensor", FileDataSource.FromDataFolder("thermal_backhall.csv")),
            new ReplayDevice(messageBus, "GLS-BACKHALL-01", "GlassBreakSensor", FileDataSource.FromDataFolder("glassbreak_backhall.csv")),
            new ReplayDevice(messageBus, "CAM-LOBBY-01", "CctvCamera", FileDataSource.FromDataFolder("camera_lobby.csv")),
            new ReplayDevice(messageBus, "SOS-TELLER-01", "EmergencySosButton", FileDataSource.FromDataFolder("sos_teller.csv"))
        };
    }

    private void SendMessages_Click(object sender, RoutedEventArgs e)
    {
        foreach (ReplayDevice replayDevice in replayDevices)
        {
            replayDevice.Tick();
        }
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

    private void AddMessage(DeviceMessage message)
    {
        DeviceMessageLog? deviceLog = DeviceLogs.FirstOrDefault(
            log => string.Equals(log.DeviceId, message.DeviceId, StringComparison.OrdinalIgnoreCase));

        if (deviceLog is null)
        {
            deviceLog = new DeviceMessageLog(message.DeviceId, message.ModuleName);
            DeviceLogs.Add(deviceLog);
        }

        deviceLog.Messages.Add(message);
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