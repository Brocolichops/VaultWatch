// Where a device gets its simulated readings from (US-3, REQ-NFR-003).
// FileDataSource reads a looped .csv file; an SQLite version can be added later
// (Strategy pattern) without changing any device class.
public interface IDataSource
{
    // Returns the next data line, wrapping back to the first row after the last one.
    // Returns null only when there is no data at all (missing, unreadable or empty file).
    string? NextLine();

    // Goes back to the first data row.
    void Reset();
}
