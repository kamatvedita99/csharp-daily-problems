/*
 * Day 06 — Filtering and Sorting
 *
 * Problem:
 * Filter and sort a list of instruments:
 * - type, exchange, currency are optional — if null, don't filter on that field
 * - activeOnly = true means only return active instruments
 * - sortBy sorts ascending by that field ("name", "type", "exchange")
 * - If sortBy is unrecognised, default to sorting by name
 * - Use LINQ, no foreach loops
 */

public class Instrument
{
    public Guid InstrumentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class Solution
{
    public List<Instrument> FilterInstruments(
        List<Instrument> instrumentsList,
        string? type,
        string? exchange,
        string? currency,
        bool activeOnly,
        string sortBy)
    {
        if (!instrumentsList.Any())
            return instrumentsList;

        var instruments = instrumentsList.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(type))
            instruments = instruments.Where(i =>
                i.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(exchange))
            instruments = instruments.Where(i =>
                i.Exchange.Equals(exchange, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(currency))
            instruments = instruments.Where(i =>
                i.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase));

        if (activeOnly)
            instruments = instruments.Where(i => i.IsActive);

        instruments = sortBy.ToLower() switch
        {
            "type"     => instruments.OrderBy(i => i.Type),
            "exchange" => instruments.OrderBy(i => i.Exchange),
            _          => instruments.OrderBy(i => i.Name)
        };

        return instruments.ToList();
    }
}

/*
 * Key learnings:
 * - AsEnumerable() converts List to IEnumerable so you can reassign with chained filters
 * - IEnumerable uses deferred execution — filters don't run until .ToList()
 * - Chain all filters, call .ToList() ONCE at the end — not after each filter
 * - Calling .ToList() after each Where = multiple traversals (bad)
 * - StringComparison.OrdinalIgnoreCase for case-insensitive string comparison
 * - Switch expression (C# 8) — cleaner than if/else for value mapping
 * - _ is the discard pattern — replaces default in switch expressions
 * - isFirstCharacter flag over i==0 — communicates intent, not just mechanics
 *
 * Deferred execution:
 * query.Where(...) → no traversal, adds filter to chain
 * query.ToList()   → ONE traversal, applies all filters + sort
 */
