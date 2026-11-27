using System;

interface ISensor
{
    void SendMessage();
}

class MotionSensor : ISensor
{
    private readonly MessageBus messageBus;
    private readonly string deviceId;

    public MotionSensor(MessageBus messageBus, string deviceId)
    {
        this.messageBus = messageBus;
        this.deviceId = deviceId;
    }

    public void SendMessage()
    {
        bool motionDetected = true;

        messageBus.Publish(new DeviceMessage(
            deviceId, "MotionSensor", "MotionDetected", motionDetected.ToString()));
    }
}

class TemperatureSensor : ISensor
{
    private readonly MessageBus messageBus;
    private readonly string deviceId;

    public TemperatureSensor(MessageBus messageBus, string deviceId)
    {
        this.messageBus = messageBus;
        this.deviceId = deviceId;
    }

    public void SendMessage()
    {
        decimal temperatureCelsius = 21.5m;

        messageBus.Publish(new DeviceMessage(
            deviceId, "TemperatureSensor", "Reading", $"{temperatureCelsius} C"));
    }
}
