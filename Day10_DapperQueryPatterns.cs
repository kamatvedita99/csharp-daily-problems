/*
 * Day 10 — Dapper Query Patterns
 *
 * Problem:
 * Implement GetVendorsAsync and GetVendorByIdAsync using Dapper.
 * Understand how Dapper differs from EF Core for read queries.
 */

using Dapper;
using InstrumentCatalogue.Core.Models;
using InstrumentCatalogue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class VendorRepository
{
    private readonly CatalogueDbContext _dbContext;

    public VendorRepository(CatalogueDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ICollection<Vendor>> GetVendorsAsync(
        CancellationToken cancellationToken = default)
    {
        using var connection = _dbContext.Database.GetDbConnection();

        // May need to open connection explicitly when using EF Core's GetDbConnection()
        // await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            commandText: "SELECT vendor_id, name, short_code, is_active FROM vendors",
            parameters: null,
            cancellationToken: cancellationToken
        );

        var vendors = await connection.QueryAsync<Vendor>(command);
        return vendors.ToList();
    }

    public async Task<Vendor?> GetVendorByIdAsync(
        int vendorId,
        CancellationToken cancellationToken = default)
    {
        using var connection = _dbContext.Database.GetDbConnection();

        var command = new CommandDefinition(
            commandText: "SELECT vendor_id, name, short_code, is_active FROM vendors WHERE vendor_id = @vendor_id",
            parameters: new { vendor_id = vendorId },
            cancellationToken: cancellationToken
        );

        return await connection.QuerySingleOrDefaultAsync<Vendor?>(command);
    }
}

/*
 * ─────────────────────────────────────────────
 * Dapper vs EF Core — when to use which
 * ─────────────────────────────────────────────
 *
 * EF Core (writes and simple reads):
 *   await _context.AddAsync(entity);
 *   await _context.SaveChangesAsync();
 *   await _context.Set<T>().FindAsync(id);
 *   + Change tracking, migrations, relationships
 *   - Slower for complex reads, less SQL control
 *
 * Dapper (complex reads):
 *   await connection.QueryAsync<T>(sql);
 *   await connection.QuerySingleOrDefaultAsync<T>(sql);
 *   + Raw SQL, fast, full control, great for JOINs
 *   - No change tracking, no migrations, manual SQL
 *
 * Pattern in this project:
 *   Writes    → EF Core (AddAsync, Update, SaveChangesAsync)
 *   Reads     → Dapper (QueryAsync, QuerySingleOrDefaultAsync)
 *
 * ─────────────────────────────────────────────
 * CommandDefinition
 * ─────────────────────────────────────────────
 *
 * Wraps all query options in one struct:
 *   commandText       → your SQL
 *   parameters        → anonymous object or DynamicParameters
 *   cancellationToken → propagates cancellation to DB driver
 *   transaction       → pass a transaction if needed
 *   commandTimeout    → override default timeout
 *
 * Always use CommandDefinition in production code —
 * ensures cancellation token is properly propagated to Postgres.
 *
 * ─────────────────────────────────────────────
 * Parameterised queries — always use them
 * ─────────────────────────────────────────────
 *
 * NEVER concatenate user input into SQL:
 *   "SELECT * FROM vendors WHERE vendor_id = " + vendorId  ← SQL injection risk
 *
 * ALWAYS use parameters:
 *   "SELECT * FROM vendors WHERE vendor_id = @vendor_id"
 *   parameters: new { vendor_id = vendorId }
 *
 * Dapper replaces @vendor_id with the parameterised value.
 * The DB driver sends them separately — no injection possible.
 *
 * ─────────────────────────────────────────────
 * Column mapping
 * ─────────────────────────────────────────────
 *
 * Dapper maps columns to properties by name.
 * snake_case columns → PascalCase properties — Npgsql handles this automatically.
 * vendor_id → VendorId ✅
 * is_active → IsActive ✅
 *
 * If mapping ever breaks, use explicit aliases in SQL:
 *   SELECT vendor_id as "VendorId", name as "Name" FROM vendors
 *
 * ─────────────────────────────────────────────
 * QueryAsync vs QuerySingleOrDefaultAsync
 * ─────────────────────────────────────────────
 *
 * QueryAsync<T>               → returns IEnumerable<T>, 0 or more rows
 * QuerySingleOrDefaultAsync<T> → returns T? — null if not found, throws if 2+ rows
 * QueryFirstOrDefaultAsync<T>  → returns T? — null if not found, first row if 2+
 * QuerySingleAsync<T>          → throws if 0 or 2+ rows — use when exactly 1 expected
 *
 * For GetById — use QuerySingleOrDefaultAsync.
 * Returns null cleanly if not found. Throws if somehow 2 rows match (shouldn't happen with PK).
 *
 * ─────────────────────────────────────────────
 * Connection management
 * ─────────────────────────────────────────────
 *
 * GetDbConnection() from EF Core returns the underlying connection.
 * It may be closed — you might need to open it explicitly:
 *   await connection.OpenAsync(cancellationToken);
 *
 * using var connection — ensures disposal after use.
 * Don't share connections across async calls — get a fresh one per operation.
 *
 * ─────────────────────────────────────────────
 * SELECT specific columns vs SELECT *
 * ─────────────────────────────────────────────
 *
 * Always select specific columns — not SELECT *
 * Reasons:
 * 1. Only fetch what you need — less network/memory overhead
 * 2. Explicit — breaking schema changes caught at query time not mapping time
 * 3. Documents what the query actually needs
 *
 * Exception: internal admin queries where you want everything
 */
