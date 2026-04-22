namespace Popocatepetl.Domain.Interfaces;

/// <summary>Contract for parsing raw CSV text into structured rows.</summary>
public interface ICsvParser
{
    /// <summary>
    /// Parses csvContent and returns each row as an array of field values.
    /// The first element is the header row when one is present.
    /// </summary>
    IReadOnlyList<string[]> Parse(string csvContent);
}
