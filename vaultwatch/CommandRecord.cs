// The three possible outcomes of a command (FUN-015)
public enum CommandResult
{
    Accepted,
    Rejected,
    Failed
}

// One command sent to a device and what happened to it (INT-002)
public class CommandRecord
{
    public CommandRecord(string deviceId, string action, string issuedBy, CommandResult result)
    {
        DeviceId = deviceId;
        Action = action;
        IssuedBy = issuedBy;
        Result = result;
    }

    public string DeviceId { get; }

    public string Action { get; }

    public string IssuedBy { get; }

    public CommandResult Result { get; }
}