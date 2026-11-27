using System.Globalization;

// One parsed row of a device data file: tick,value,status (INT-004)
public readonly record struct DataSample(int Tick, double Value, string Status);

public static class CsvSampleParser
{
    // Status values from INT-001
    private static readonly string[] ValidStatuses = { "Normal", "Warning", "Alarm", "Fault" };

    // Returns false for any malformed line instead of throwing (NFR-006).
    public static bool TryParse(string? line, out DataSample sample)
    {
        sample = default;

        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        string[] parts = line.Split(',');
        if (parts.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int tick))
        {
            return false;
        }

        if (!double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
            || double.IsNaN(value)
            || double.IsInfinity(value))
        {
            return false;
        }

        string? status = ValidStatuses.FirstOrDefault(
            s => string.Equals(s, parts[2].Trim(), StringComparison.OrdinalIgnoreCase));
        if (status is null)
        {
            return false;
        }

        sample = new DataSample(tick, value, status);
        return true;
    }
}
