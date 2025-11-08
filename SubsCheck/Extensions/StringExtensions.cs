using System.Text.RegularExpressions;

namespace SubsCheck.Extensions;
public static class StringExtensions
{
    public static bool ContainsIsolatedText(this string text, string target)
        => Regex.Split(text.ToLower(), @"[^\w']").Contains(target.ToLower());
}
