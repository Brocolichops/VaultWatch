// A device that replays its own data file (US-3). One tick = one line from the data source.
// Works for any sensor type, so scenarios (break-in, SOS, camera offline) are just CSV files.
internal sealed class ReplayDevice : ISensor
{
    private readonly MessageBus messageBus;
    private readonly string deviceId;
    private readonly string moduleName;
    private readonly IDataSource source;

    public ReplayDevice(MessageBus messageBus, string deviceId, string moduleName, IDataSource source)
    {
        this.messageBus = messageBus;
        this.deviceId = deviceId;
        this.moduleName = moduleName;
        this.source = source;
    }

    // The last reading that parsed correctly. Kept when a bad line is skipped (NFR-006).
    public DataSample? LastGoodSample { get; private set; }

    public void Tick()
    {
        string? line;
        try
        {
            line = source.NextLine();
        }
        catch (Exception ex)
        {
            Warn($"Could not read the data source: {ex.Message}");
            return;
        }

        if (line is null)
        {
            Warn("No data available from the data source");
            return;
        }

        if (!CsvSampleParser.TryParse(line, out DataSample sample))
        {
            Warn($"Skipped malformed line: {line}");
            return;
        }

        LastGoodSample = sample;
        messageBus.Publish(new TelemetryMessage(deviceId, moduleName, sample));
    }

    // Lets the existing "SEND SENSOR MESSAGES" button drive this device.
    public void SendMessage()
    {
        Tick();
    }

    private void Warn(string text)
    {
        messageBus.Publish(new DeviceMessage(deviceId, moduleName, "Warning", text));
    }
}
