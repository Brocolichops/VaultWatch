#if CONSOLE_REFERENCE
using System;
using System.Collections.Generic;

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
#endif
