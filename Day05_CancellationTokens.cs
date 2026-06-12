/*
 * Day 05 — Cancellation Tokens
 *
 * Problem:
 * Add cancellation token support to the market value service:
 * - Accept CancellationToken with default value
 * - Pass it down to the repository call
 * - Handle OperationCanceledException separately — log at Info, rethrow as-is
 * - Keep all existing validation and error handling from Day 04
 */

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

    public async Task<decimal?> GetTotalMarketValueAsync(
        string portfolioId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(portfolioId))
            throw new ArgumentException("Cannot be null or empty", nameof(portfolioId));

        try
        {
            var positions = await _repository.GetByPortfolioAsync(
                portfolioId,
                cancellationToken);  // pass token down to DB call

            if (!positions.Any())
                return null;

            return positions.Sum(p => p.Quantity * p.MarketPrice);
        }
        catch (OperationCanceledException ex)
        {
            // Cancellation is expected behaviour — log at Info, not Error
            _logger.LogInformation(ex,
                "Request cancelled for portfolio {PortfolioId}",
                portfolioId);
            throw;  // rethrow as-is — preserve original exception type
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
 * - CancellationToken cancellationToken = default — always provide default
 * - Pass token to EVERY async call in the method — that's the whole point
 * - OperationCanceledException must be caught BEFORE general Exception catch
 * - throw (not throw ex) — preserves stack trace
 * - throw ex — resets stack trace, lose the original call location
 * - Cancellation is NOT an error — log at Info, not Error
 * - Manual IsCancellationRequested checks only for CPU-bound loops, not async IO
 *
 * How cancellation works:
 * Client disconnects → TCP closes → ASP.NET Core signals HttpContext.RequestAborted
 * → CancellationToken cancelled → propagates down to DB driver → Postgres query cancelled
 * ASP.NET Core auto-binds HttpContext.RequestAborted to controller CancellationToken params
 *
 * Dapper with CancellationToken — use CommandDefinition:
 * var cmd = new CommandDefinition(sql, parameters, cancellationToken: ct);
 * await connection.QueryAsync<T>(cmd);
 */
