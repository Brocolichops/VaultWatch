using System.IO;
// Reads a device's .csv data file (header tick,value,status) and loops at the end (US-3, FUN-002).
public sealed class FileDataSource : IDataSource
{
    private readonly List<string> rows = new List<string>();
    private int index;

    public FileDataSource(string filePath)
    {
        FilePath = filePath;

        try
        {
            bool sawFirstLine = false;

            foreach (string raw in File.ReadLines(filePath))
            {
                string line = raw.Trim();
                if (line.Length == 0)
                {
                    continue; // blank lines are ignored, not counted as rows
                }

                if (!sawFirstLine)
                {
                    sawFirstLine = true;
                    if (IsHeader(line))
                    {
                        continue;
                    }
                }

                rows.Add(line);
            }

            if (rows.Count == 0)
            {
                Error = "The data file has no data rows.";
            }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            // A missing or unreadable file must not crash the app (FUN-012). NextLine returns null.
            Error = ex.Message;
        }
    }

    public string FilePath { get; }

    public bool IsAvailable => rows.Count > 0;

    public string? Error { get; }

    // Data files are copied beside the .exe in a "data" folder.
    public static FileDataSource FromDataFolder(string fileName)
    {
        return new FileDataSource(Path.Combine(AppContext.BaseDirectory, "data", fileName));
    }

    public string? NextLine()
    {
        if (rows.Count == 0)
        {
            return null;
        }

        string line = rows[index];
        index = (index + 1) % rows.Count; // loop back to row 1 after the last row
        return line;
    }

    public void Reset()
    {
        index = 0;
    }

    private static bool IsHeader(string line)
    {
        return string.Equals(line.Replace(" ", string.Empty), "tick,value,status", StringComparison.OrdinalIgnoreCase);
    }
}
