/*
 * Day 03 — Async Market Value
 *
 * Problem:
 * Write a properly async service method that:
 * - Fetches positions for a portfolio from a repository
 * - Returns null if no positions found
 * - Calculates total market value (Quantity * MarketPrice)
 * - No .Result or .Wait() — properly async throughout
 */

public interface IPositionRepository
{
    Task<List<Position>> GetByPortfolioAsync(string portfolioId);
}

public class Position
{
    public string PortfolioId { get; set; }
    public string InstrumentId { get; set; }
    public decimal Quantity { get; set; }
    public decimal MarketPrice { get; set; }
}

public class PositionService
{
    private readonly IPositionRepository _repository;

    public PositionService(IPositionRepository repository)
    {
        _repository = repository;
    }

    public async Task<decimal?> GetTotalMarketValueAsync(string portfolioId)
    {
        var positions = await _repository.GetByPortfolioAsync(portfolioId);

        if (!positions.Any())
            return null;

        return positions.Sum(p => p.Quantity * p.MarketPrice);
    }
}

/*
 * Key learnings:
 * - await unwraps Task<T> — gives you T directly
 * - .ToList() is called on IEnumerable, NOT on Task — await first, then LINQ
 * - async/await is NOT parallel processing — it frees the thread during IO wait
 * - Never use .Result or .Wait() — causes deadlocks in ASP.NET Core
 * - Return Task<decimal?> — nullable because portfolio might have no positions
 */
