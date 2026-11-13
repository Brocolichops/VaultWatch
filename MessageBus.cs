using System;
using System.Collections.Generic;

// A general message from any RTU or connected module.
public class DeviceMessage
{
    public string DeviceId { get; set; }
    public string ModuleName { get; set; }
    public string MessageType { get; set; }
    public string Value { get; set; }
    public DateTime Time { get; set; }

    public DeviceMessage(
        string deviceId,
        string moduleName,
        string messageType,
        string value)
    {
        DeviceId = deviceId;
        ModuleName = moduleName;
        MessageType = messageType;
        Value = value;
        Time = DateTime.UtcNow;
    }
}

class MessageBus
{
    private List<Action<DeviceMessage>> subscribers =
        new List<Action<DeviceMessage>>();

    public void Subscribe(Action<DeviceMessage> handler)
    {
        subscribers.Add(handler);
    }

    public void Publish(DeviceMessage message)
    {
        foreach (Action<DeviceMessage> handler in subscribers)
        {
            handler(message);
        }
    }
}
