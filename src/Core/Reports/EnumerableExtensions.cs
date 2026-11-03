public static class EnumerableExtensions
{
    /// Власний метод-розширення LINQ: повертає перші N елементів за спаданням ключа.
    public static IEnumerable<T> TopBy<T>(
        this IEnumerable<T> source,
        Func<T, int> keySelector,
        int count) =>
        source.TopBy<T, int>(keySelector, count);

    /// Узагальнений метод-розширення LINQ: повертає перші N елементів за спаданням довільного ключа.
    public static IEnumerable<T> TopBy<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector,
        int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if (count == 0)
            return Enumerable.Empty<T>();

        return source.OrderByDescending(keySelector).Take(count);
    }
}

