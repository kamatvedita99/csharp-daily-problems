/*
 * Day 02 — Portfolio Summary
 *
 * Problem:
 * Given a list of positions, return a summary per portfolio:
 * - InstrumentCount: number of DISTINCT instruments
 * - TotalMarketValue: sum of Quantity * MarketPrice
 * - LargestPosition: highest single Quantity * MarketPrice
 * - Use LINQ, no foreach loops
 */

public class Position
{
    public string PortfolioId { get; set; }
    public string InstrumentId { get; set; }
    public decimal Quantity { get; set; }
    public decimal MarketPrice { get; set; }
}

public class PortfolioSummary
{
    public string PortfolioId { get; set; }
    public int InstrumentCount { get; set; }
    public decimal TotalMarketValue { get; set; }
    public decimal LargestPosition { get; set; }
}

public class Solution
{
    public List<PortfolioSummary> GetPortfolioSummaries(List<Position> positions)
    {
        return positions
            .GroupBy(p => p.PortfolioId)
            .Select(group => new PortfolioSummary
            {
                PortfolioId      = group.Key,
                InstrumentCount  = group.Select(pos => pos.InstrumentId)
                                        .Distinct()
                                        .Count(),
                TotalMarketValue = group.Sum(pos => pos.Quantity * pos.MarketPrice),
                LargestPosition  = group.Max(pos => pos.Quantity * pos.MarketPrice)
            })
            .ToList();
    }
}

/*
 * Key learnings:
 * - GroupBy creates buckets — each group has a Key and a mini-list of items
 * - Two levels of LINQ: outer operates on groups, inner operates on items in a group
 * - Distinct() before Count() for unique instrument count
 * - Max() with a selector — Max(pos => pos.Quantity * pos.MarketPrice)
 * - Avoid reusing the same lambda variable name across scopes — use descriptive names
 */
