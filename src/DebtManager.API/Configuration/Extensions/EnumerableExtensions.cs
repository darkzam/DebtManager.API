namespace DebtManager.API.Configuration.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool HasItems<T>(this IEnumerable<T> collection)
        {
            return collection?.Any() ?? false;
        }
    }
}
