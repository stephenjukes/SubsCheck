namespace SubsCheck.Extensions;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> Concat<T>(this T member, IEnumerable<T> collection)
        => new T[] { member }.Concat(collection);

    public static IEnumerable<T> Concat<T>(this IEnumerable<T> collection, T member)
        => collection.Concat([member]);
}
