/*
 * Day 14 — The Boundary of What Generic Constraints Protect
 *
 * Day 13 was about WHEN to reach for a generic constraint.
 * Day 14 is about exactly WHERE that protection stops, once you have one.
 *
 * No code to write today — this is a reasoning exercise, worked through
 * BEFORE touching the IDE, using the ITimeStampAudit setup from Day 13.
 */

public interface ITimeStampAudit
{
    DateTime CreatedAtUtc { get; set; }
    DateTime LastUpdatedAtUtc { get; set; }
}

public static class AuditUtilityExtensionHelper
{
    public static T StampCreated<T>(this T entity) where T : ITimeStampAudit
    {
        entity.CreatedAtUtc = DateTime.UtcNow;
        entity.LastUpdatedAtUtc = DateTime.UtcNow;
        return entity;
    }

    public static T StampUpdated<T>(this T entity) where T : ITimeStampAudit
    {
        entity.LastUpdatedAtUtc = DateTime.UtcNow;
        return entity;
    }
}

// Deliberately does NOT implement ITimeStampAudit
public class VendorInterfaceSummary
{
    public int VendorInterfaceId { get; set; }
    public string Name { get; set; } = string.Empty;
}

/*
 * ─────────────────────────────────────────────
 * Question 1
 * ─────────────────────────────────────────────
 *
 *   var summary = new VendorInterfaceSummary();
 *   summary.StampCreated();
 *
 * Does this compile? If not, at what point does it fail —
 * while typing, at build time, or only at runtime?
 *
 * ANSWER:
 * Fails immediately, at compile time — and in practice even earlier,
 * the IDE flags it the instant you write the line, before you ever
 * press build. The generic constraint `where T : ITimeStampAudit` is
 * checked against the type argument as soon as it's known. Since
 * VendorInterfaceSummary has neither CreatedAtUtc nor LastUpdatedAtUtc,
 * it cannot satisfy the constraint, and the compiler refuses to
 * instantiate StampCreated<VendorInterfaceSummary> at all.
 *
 * This is the entire point of generic constraints: push the failure as
 * early as possible in the development timeline, not into a runtime
 * exception discovered in production.
 *
 * ─────────────────────────────────────────────
 * Question 2
 * ─────────────────────────────────────────────
 *
 * Can a class implement ITimeStampAudit while only providing ONE of
 * the two required members? What does the compiler do?
 *
 * ANSWER:
 * No. C# interfaces have no concept of "optional" members and no
 * partial implementation. If a class declares `: ITimeStampAudit` but
 * is missing even one declared member — regardless of whether that
 * member is nullable or not — the compiler rejects the class
 * declaration itself, with an error naming the exact missing member.
 * This failure happens at the class declaration, independent of
 * whether the class is ever used generically anywhere.
 *
 * ─────────────────────────────────────────────
 * Question 3 — the one that matters most
 * ─────────────────────────────────────────────
 *
 * If you accidentally write vendor.StampUpdated() inside ToDomain()
 * (meant for brand new records) instead of vendor.StampCreated() —
 * does the compiler catch this?
 *
 * ANSWER:
 * No. Both StampCreated() and StampUpdated() are fully valid, type-safe
 * calls on a Vendor. The compiler has no concept of "this is a new
 * record" vs "this is an existing record being modified" — that's
 * business intent, not a type. Calling the wrong-but-valid method
 * compiles cleanly and produces a silent logic bug: a new Vendor row
 * with CreatedAtUtc left at DateTime.MinValue, discovered only later,
 * possibly in production, possibly never.
 *
 * ─────────────────────────────────────────────
 * The actual lesson
 * ─────────────────────────────────────────────
 *
 * Generic constraints protect SHAPE — "does this type have the
 * members I need." They cannot protect INTENT — "did the developer
 * call the semantically correct method for this situation."
 *
 * This is exactly why method NAMING carries real weight, not just
 * style. StampCreated() vs StampUpdated() — clear, distinct names are
 * the only defence against Question 3's bug, because the type system
 * has genuinely run out of road by that point. No interface, no
 * constraint, no clever generic trick closes this gap. Only careful
 * naming, code review, and tests do.
 *
 * Knowing exactly where a tool's protection ends is as important as
 * knowing how to use the tool — it tells you where to stop relying on
 * the compiler and start relying on discipline.
 */
