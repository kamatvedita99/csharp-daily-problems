/*
 * Day 09 — Records, Immutable Types, and Mapping
 *
 * Problem:
 * Write a static mapper class that maps between domain models and DTOs.
 * Understand when and why to use records vs classes.
 */

public class Vendor
{
    public int VendorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }
}

public record VendorResponse(int VendorId, string Name, string ShortCode, bool IsActive);

public class CreateVendorRequest
{
    public string Name { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
}

public static class VendorMapper
{
    public static VendorResponse ToResponse(Vendor vendor)
    {
        ArgumentNullException.ThrowIfNull(vendor);
        // Records use positional constructor — values in order, not by property name
        return new VendorResponse(vendor.VendorId, vendor.Name, vendor.ShortCode, vendor.IsActive);
    }

    public static Vendor ToDomain(CreateVendorRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new Vendor
        {
            VendorId = 0,              // DB generates this — leave as 0
            Name = request.Name,
            ShortCode = request.ShortCode,
            IsActive = true,           // always active on creation
            CreatedAtUtc = DateTime.UtcNow,
            LastUpdatedAtUtc = DateTime.UtcNow
        };
    }
}

/*
 * ─────────────────────────────────────────────
 * Records — what, why, when
 * ─────────────────────────────────────────────
 *
 * WHAT is a record?
 * A record is a special class designed for immutable data.
 * Once created, the values cannot be changed.
 *
 * Two syntax styles:
 *
 * Positional (what we use):
 *   public record VendorResponse(int VendorId, string Name, string ShortCode, bool IsActive);
 *   One line. Compiler generates constructor, properties, equality, ToString automatically.
 *   Create: new VendorResponse(1, "Bloomberg", "BBG", true)
 *
 * Standard (verbose):
 *   public record VendorResponse
 *   {
 *       public int VendorId { get; init; }
 *       public string Name { get; init; }
 *   }
 *   `init` means can only be set during construction — immutable after.
 *
 * WHY use records?
 *
 * 1. Immutability — once a VendorResponse is created, nobody can change its values.
 *    This is exactly what you want for API responses — create it, return it, done.
 *    No risk of something modifying the response object accidentally.
 *
 * 2. Value equality — two records with the same values are equal.
 *    new VendorResponse(1, "Bloomberg", "BBG", true)
 *    == new VendorResponse(1, "Bloomberg", "BBG", true)  → TRUE
 *    For classes this would be FALSE (reference equality by default).
 *    Useful for testing — compare expected vs actual response without custom equality.
 *
 * 3. Less code — positional record in one line vs 10 lines of class boilerplate.
 *
 * WHEN to use records vs classes:
 *
 * Use RECORD when:
 *   - Data is read-only after creation (API responses, query results)
 *   - You want value-based equality
 *   - No business logic on the object
 *   → VendorResponse, InstrumentResponse, PagedResult<T>
 *
 * Use CLASS when:
 *   - Data needs to change after creation
 *   - You need validation attributes ([Required], [MaxLength])
 *   - Object has behaviour / methods
 *   → CreateVendorRequest, domain models (Vendor, Instrument)
 *   → Why request DTOs are classes: [Required] and model binding work better
 *
 * ─────────────────────────────────────────────
 * DateTime.UtcNow vs DateTime.Now
 * ─────────────────────────────────────────────
 *
 * DateTime.Now    — local machine time. If your server is in Mumbai,
 *                   this is IST. If deployed to AWS US-East, this is EST.
 *                   Same action, different timestamps. Data inconsistency.
 *
 * DateTime.UtcNow — always UTC regardless of server location.
 *                   Consistent everywhere. Convert to local time in the UI if needed.
 *
 * Rule: always store UTC in the database. Always use DateTime.UtcNow in code.
 *
 * ─────────────────────────────────────────────
 * Static mapper class pattern
 * ─────────────────────────────────────────────
 *
 * VendorMapper is a static class — no instance needed, just call the methods directly.
 * VendorMapper.ToResponse(vendor)
 * VendorMapper.ToDomain(request)
 *
 * In the Application layer:
 *   Service receives CreateVendorRequest from controller
 *   Maps to Vendor domain model via ToDomain()
 *   Passes to repository for saving
 *   Gets saved Vendor back
 *   Maps to VendorResponse via ToResponse()
 *   Returns VendorResponse to controller
 *
 * Alternative: use AutoMapper library (maps automatically by convention).
 * We use manual mappers because:
 *   - Explicit is better than magic for learning
 *   - No hidden runtime errors from misconfigured mappings
 *   - Easy to debug — you can see exactly what maps to what
 */
