using System.Globalization;

// A device reading on the message bus. It is a DeviceMessage, so the existing message grid
// shows it unchanged, but rules (alarms) can read the number and status without parsing text.
public class TelemetryMessage : DeviceMessage
{
    public TelemetryMessage(string deviceId, string moduleName, DataSample sample)
        : base(deviceId, moduleName, "Reading",
               $"{sample.Value.ToString(CultureInfo.InvariantCulture)} ({sample.Status})")
    {
        Tick = sample.Tick;
        NumericValue = sample.Value;
        Status = sample.Status;
    }

    public int Tick { get; }

    public double NumericValue { get; }
}
