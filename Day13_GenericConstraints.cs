/*
 * Day 13 — Generic Constraints: When They Earn Their Place
 *
 * This problem went through two versions, and BOTH versions are worth
 * keeping, because the contrast is the actual lesson.
 *
 * VERSION 1 (rejected as ceremonial — kept here as a teaching example):
 *
 *   public interface IHasId { int Id { get; } }
 *
 *   public static class Finder
 *   {
 *       public static T? FindById<T>(List<T> items, int id) where T : IHasId
 *       {
 *           return items.SingleOrDefault(i => i.Id == id);
 *       }
 *   }
 *
 * Why this was rejected: it saves almost nothing. The interface, the
 * generic constraint, and the static helper class together replace a
 * single inline lambda — items.SingleOrDefault(i => i.Id == id) — that
 * you could just write directly wherever needed. Generic constraints
 * should earn their complexity by removing REAL repeated logic, not
 * by wrapping a one-liner in ceremony.
 *
 * VERSION 2 (the real version — earns its place):
 * Three actual project models all need CreatedAtUtc and LastUpdatedAtUtc
 * stamped consistently on create, and LastUpdatedAtUtc only on update.
 * Without this pattern, every mapper repeats:
 *     CreatedAtUtc = DateTime.UtcNow,
 *     LastUpdatedAtUtc = DateTime.UtcNow
 * — and it's easy to forget one of the two fields on any given mapper.
 */

public class Vendor : ITimeStampAudit
{
    public int VendorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }
}

public class Symbology : ITimeStampAudit
{
    public int SymbologyId { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }
}

public class VendorInterface : ITimeStampAudit
{
    public int VendorInterfaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }
}

public interface ITimeStampAudit
{
    DateTime CreatedAtUtc { get; set; }
    DateTime LastUpdatedAtUtc { get; set; }
}

public static class AuditUtilityExtensionHelper
{
    // Used on CREATE — a brand new entity needs both timestamps set together
    public static T StampCreated<T>(this T entity) where T : ITimeStampAudit
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.CreatedAtUtc = DateTime.UtcNow;
        entity.LastUpdatedAtUtc = DateTime.UtcNow;
        return entity;
    }

    // Used on UPDATE — never touch CreatedAtUtc, that would rewrite history
    public static T StampUpdated<T>(this T entity) where T : ITimeStampAudit
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.LastUpdatedAtUtc = DateTime.UtcNow;
        return entity;
    }
}

public static class VendorMapper
{
    public static Vendor ToDomain(CreateVendorRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var vendor = new Vendor
        {
            VendorId = 0,
            Name = request.Name
        };

        return vendor.StampCreated();
    }
}

/*
 * ─────────────────────────────────────────────
 * Why this version earns the complexity
 * ─────────────────────────────────────────────
 *
 * 1. Removes real duplication — every audited entity's mapper drops two
 *    manually-typed lines, across Vendor, Symbology, VendorInterface,
 *    and every future entity that implements ITimeStampAudit.
 *
 * 2. Removes a real bug class — it's no longer possible to forget to
 *    stamp ONE of the two timestamps on create, since they're always
 *    set together inside one method.
 *
 * 3. The constraint does real work — `where T : ITimeStampAudit`
 *    guarantees at compile time that CreatedAtUtc and LastUpdatedAtUtc
 *    exist before the method body ever touches them. Without the
 *    constraint, the compiler would reject entity.CreatedAtUtc outright,
 *    since T could be anything.
 *
 * ─────────────────────────────────────────────
 * The `this T entity` keyword — extension methods
 * ─────────────────────────────────────────────
 *
 * Without `this`:
 *   public static T StampCreated<T>(T entity) where T : ITimeStampAudit
 *   // called as: AuditUtilityExtensionHelper.StampCreated(vendor)
 *
 * With `this`:
 *   public static T StampCreated<T>(this T entity) where T : ITimeStampAudit
 *   // called as: vendor.StampCreated()
 *
 * The `this` keyword on the first parameter turns a static method into
 * an extension method — callable directly on the instance, as if it
 * were a real instance method on the type, without modifying the
 * original Vendor/Symbology/VendorInterface classes at all.
 *
 * ─────────────────────────────────────────────
 * Why StampCreated and StampUpdated are SEPARATE methods,
 * not one method with a boolean flag
 * ─────────────────────────────────────────────
 *
 * Rejected design:
 *   public static T Stamp<T>(this T entity, bool isCreate) where T : ITimeStampAudit
 *   // called as: vendor.Stamp(true)   ← what does true mean here??
 *
 * "Flag arguments" like this are a known anti-pattern. The call site
 * stops being self-documenting — you have to go read the method body
 * to know what `true` actually does. Two clearly named methods instead:
 *
 *   vendor.StampCreated()   // unambiguous from the name alone
 *   vendor.StampUpdated()   // unambiguous from the name alone
 *
 * This is a small example of a much bigger rule: prefer multiple small,
 * well-named methods over one method with a flag that branches its
 * own behaviour internally.
 *
 * ─────────────────────────────────────────────
 * The core lesson from today
 * ─────────────────────────────────────────────
 *
 * Generic constraints are a tool, not a virtue. Ask before reaching for
 * one: "does this remove REAL repeated logic, or am I just wrapping a
 * one-liner in ceremony?" If the answer is the latter, write the simple
 * direct code instead. If the answer is the former — like timestamp
 * stamping across many entities — the interface and constraint earn
 * their complexity by preventing real, repeatable bugs.
 */
