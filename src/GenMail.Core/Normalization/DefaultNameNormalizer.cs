using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using GenMail.Core.Models;

namespace GenMail.Core.Normalization;

public sealed partial class DefaultNameNormalizer : INameNormalizer
{
    [GeneratedRegex("\\s+")]
    private static partial Regex WhitespaceRegex();

    public NormalizedName Normalize(string input)
    {
        string original = input;
        string collapsed = WhitespaceRegex().Replace(input.Trim(), " ");
        string normalized = RemoveAccents(collapsed).Replace('đ', 'd').Replace('Đ', 'D').ToLowerInvariant();
        string[] tokens = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        string first = tokens.Length > 0 ? tokens[0] : string.Empty;
        string last = tokens.Length > 1 ? tokens[^1] : first;
        string middle = tokens.Length > 2 ? string.Concat(tokens.Skip(1).Take(tokens.Length - 2)) : string.Empty;
        string all = string.Concat(tokens);
        string reverseAll = string.Concat(tokens.Reverse());

        return new NormalizedName(original, normalized, first, middle, last, all, reverseAll, false);
    }

    private static string RemoveAccents(string input)
    {
        string formD = input.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new();
        foreach (char ch in formD)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
