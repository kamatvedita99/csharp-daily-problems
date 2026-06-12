/*
 * Day 07 — Extension Methods
 *
 * Problem:
 * Write two extension methods:
 *
 * 1. ToSnakeCase — converts PascalCase/camelCase to snake_case
 *    "InstrumentId"  → "instrument_id"
 *    "CreatedAtUtc"  → "created_at_utc"
 *    "isActive"      → "is_active"
 *
 * 2. WhereIf — applies a filter only if a condition is true
 *    query.WhereIf(!string.IsNullOrWhiteSpace(type), i => i.Type == type)
 */

using System.Text;

public static class StringExtensions
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        bool isFirstCharacter = true;
        var sb = new StringBuilder();

        for (int i = 0; i < input.Length; i++)
        {
            if (isFirstCharacter)
            {
                isFirstCharacter = false;
                sb.Append(char.ToLower(input[i]));
            }
            else if (input[i] >= 'A' && input[i] <= 'Z')
            {
                sb.Append('_');
                sb.Append(char.ToLower(input[i]));
            }
            else
            {
                sb.Append(input[i]);
            }
        }

        return sb.ToString();
    }
}

public static class QueryExtensions
{
    public static IEnumerable<T> WhereIf<T>(
        this IEnumerable<T> source,
        bool condition,
        Func<T, bool> predicate)
    {
        if (!condition)
            return source;

        if (predicate == null)
            return source;

        return source.Where(predicate);
    }
}

/*
 * Usage in FilterInstruments — cleaner than if/else blocks:
 *
 * return instruments
 *     .AsEnumerable()
 *     .WhereIf(!string.IsNullOrWhiteSpace(type), i => i.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
 *     .WhereIf(!string.IsNullOrWhiteSpace(exchange), i => i.Exchange.Equals(exchange, StringComparison.OrdinalIgnoreCase))
 *     .WhereIf(activeOnly, i => i.IsActive)
 *     .OrderBy(i => i.Name)
 *     .ToList();
 *
 * Key learnings:
 * - Extension methods: static class, static method, first param is "this T"
 * - this keyword makes it callable on the type — "instrument".ToSnakeCase()
 * - isFirstCharacter flag communicates intent better than i == 0
 * - StringBuilder over string concatenation in loops — strings are immutable,
 *   concatenation creates a new string every iteration (O(n²) allocations)
 * - WhereIf makes conditional filter chains readable and chainable
 * - Func<T, bool> is the type for a lambda that takes T and returns bool
 * - ArgumentNullException.ThrowIfNull(predicate) — alternative to null check
 */
