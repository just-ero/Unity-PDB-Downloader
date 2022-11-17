using System.Linq;

namespace UnityPdbDl.Extensions;

internal static class StringExtensions
{
    public static string Strip(this string source, string value)
    {
        return source.Replace(value, "");
    }

    public static string Strip(this string source, char value)
    {
        return string.Concat(source.Where(c => c != value));
    }
}
