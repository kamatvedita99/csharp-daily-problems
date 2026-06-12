/*
 * Day 04 — Validation + Error Handling + Logging
 *
 * Problem:
 * Extend the async market value method with:
 * - Input validation — throw ArgumentException if portfolioId is null/empty
 * - try/catch around repository call
 * - Log errors using ILogger with structured logging
 * - Rethrow as custom PortfolioServiceException with inner exception
 */

public class PortfolioServiceException : Exception
{
    public string PortfolioId { get; }

    public PortfolioServiceException(string portfolioId, string message, Exception inner)
        : base(message, inner)
    {
        PortfolioId = portfolioId;
    }
}

public class PositionService
{
    private readonly IPositionRepository _repository;
    private readonly ILogger<PositionService> _logger;

    public PositionService(
        IPositionRepository repository,
        ILogger<PositionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<decimal?> GetTotalMarketValueAsync(string portfolioId)
    {
        if (string.IsNullOrWhiteSpace(portfolioId))
            throw new ArgumentException("Cannot be null or empty", nameof(portfolioId));

        try
        {
            var positions = await _repository.GetByPortfolioAsync(portfolioId);

            if (!positions.Any())
                return null;

            return positions.Sum(p => p.Quantity * p.MarketPrice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to get market value for portfolio {PortfolioId}",
                portfolioId);

            throw new PortfolioServiceException(
                portfolioId,
                "Failed to retrieve market value for portfolio",
                ex);
        }
    }
}

/*
 * Key learnings:
 * - ArgumentException(message, paramName) — paramName is second argument
 * - nameof(portfolioId) — refactor-safe way to get parameter name as string
 * - _logger.LogError(ex, "message {Placeholder}", value) — structured logging
 * - Named placeholders {PortfolioId} not string interpolation $"" — Serilog indexes these
 * - Custom exception wraps original with inner exception — preserves root cause
 * - Never swallow exceptions silently — always log + rethrow or handle
 */
