/*
 * Day 01 — Trade Enrichment
 *
 * Problem:
 * Given a list of trades and a list of instruments, return an enriched trade result.
 * - Trades with no matching instrument should be skipped
 * - TradeValue = Quantity * Price
 * - Return as List<EnrichedTrade>
 * - The instruments list could have 100,000 entries — solution must be O(n), not O(n²)
 * - Use LINQ, no foreach loops
 */

public class Trade
{
    public string TradeId { get; set; }
    public string InstrumentId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
}

public class Instrument
{
    public string InstrumentId { get; set; }
    public string Name { get; set; }
    public string Currency { get; set; }
}

public class EnrichedTrade
{
    public string TradeId { get; set; }
    public string InstrumentName { get; set; }
    public string Currency { get; set; }
    public decimal Quantity { get; set; }
    public decimal TradeValue { get; set; }
}

public class Solution
{
    public List<EnrichedTrade> EnrichTrades(
        List<Trade> trades,
        List<Instrument> instruments)
    {
        // Build dictionary first for O(1) lookup — avoids O(n²) nested loop
        var instrumentMap = instruments.ToDictionary(i => i.InstrumentId);

        return trades
            .Where(t => instrumentMap.ContainsKey(t.InstrumentId))
            .Select(t =>
            {
                var instrument = instrumentMap[t.InstrumentId];
                return new EnrichedTrade
                {
                    TradeId = t.TradeId,
                    InstrumentName = instrument.Name,
                    Currency = instrument.Currency,
                    Quantity = t.Quantity,
                    TradeValue = t.Quantity * t.Price
                };
            })
            .ToList();
    }
}

/*
 * Key learnings:
 * - Dictionary lookup is O(1) vs O(n) list scan
 * - ContainsKey before indexer [] avoids KeyNotFoundException
 * - Select with block body { var x = ...; return new ... } for multi-line projections
 * - ToDictionary() to convert a list into a lookup map
 */
