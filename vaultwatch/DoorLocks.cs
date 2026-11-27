using System.ComponentModel;
using System.Runtime.CompilerServices;

public class DoorLock : INotifyPropertyChanged
{
    public DoorLock(string deviceId, string name)
    {
        DeviceId = deviceId;
        Name = name;
    }

    public string DeviceId { get; }

    public string Name { get; }

    public bool IsLocked { get; private set; } = true;

    public bool IsLockdownActive { get; private set; }

    public string StatusText => IsLocked ? "LOCKED" : "UNLOCKED";

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Lock()
    {
        SetLockState(true);
    }

    public void Unlock()
    {
        SetLockState(false);
    }

    public void ActivateLockdown()
    {
        IsLockdownActive = true;
        OnPropertyChanged(nameof(IsLockdownActive));
        SetLockState(true);
    }

    private void SetLockState(bool isLocked)
    {
        if (IsLocked == isLocked)
        {
            return;
        }

        IsLocked = isLocked;
        OnPropertyChanged(nameof(IsLocked));
        OnPropertyChanged(nameof(StatusText));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
