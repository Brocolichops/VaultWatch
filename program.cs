using System;
using System.Collections.Generic;

// A general message from any RTU or connected module.
class DeviceMessage
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

class Program
{
    static void Main()
    {
        MessageBus bus = new MessageBus();

        // This listener receives every message sent through the bus.
        bus.Subscribe(DisplayMessage);

        // Example messages from different kinds of modules.
        bus.Publish(new DeviceMessage(
            "rtu-01", "DoorLock", "StateChanged", "Locked"));

        bus.Publish(new DeviceMessage(
            "rtu-02", "MotionSensor", "MotionDetected", "true"));

        bus.Publish(new DeviceMessage(
            "rtu-03", "TemperatureSensor", "Reading", "21.5 C"));
    }

    static void DisplayMessage(DeviceMessage message)
    {
        Console.WriteLine(
            message.Time.ToString("u") + " | " +
            message.DeviceId + " | " +
            message.ModuleName + " | " +
            message.MessageType + " | " +
            message.Value);
    }
}