using System.Collections.ObjectModel;

namespace VaultWatch;

public sealed class DeviceMessageLog
{
    public DeviceMessageLog(string deviceId, string moduleName)
    {
        DeviceId = deviceId;
        ModuleName = moduleName;
    }

    public string DeviceId { get; }

    public string ModuleName { get; }

    public ObservableCollection<DeviceMessage> Messages { get; } = new ObservableCollection<DeviceMessage>();
}
