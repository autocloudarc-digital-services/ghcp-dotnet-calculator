#nullable enable

namespace calculator.web.Services;

/// <summary>Stores the 50 most recent successful calculations in the current circuit only.</summary>
public sealed class HistoryService
{
    private readonly List<CalculationRecord> entries = new();

    /// <summary>Gets a read-only newest-first view of session history.</summary>
    public IReadOnlyList<CalculationRecord> Entries => entries.AsReadOnly();

    /// <summary>Adds a successful calculation and removes entries beyond the session limit.</summary>
    /// <param name="record">The completed calculation.</param>
    public void Add(CalculationRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        entries.Insert(0, record);
        if (entries.Count > 50) entries.RemoveAt(entries.Count - 1);
    }

    /// <summary>Clears session history without resetting the current calculation.</summary>
    public void Clear() => entries.Clear();
}
