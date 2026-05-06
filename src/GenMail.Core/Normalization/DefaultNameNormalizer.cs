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
        string normalizedText = RemoveAccents(collapsed).Replace('đ', 'd').Replace('Đ', 'D').ToLowerInvariant();

        List<string> tokens = normalizedText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(static token => token.Trim())
            .Where(static token => token.Length > 0)
            .ToList();

        string first = tokens.Count > 0 ? tokens[0] : string.Empty;
        string last = tokens.Count > 1 ? tokens[^1] : first;
        string middle = tokens.Count > 2 ? string.Concat(tokens.Skip(1).Take(tokens.Count - 2)) : string.Empty;
        string all = string.Concat(tokens);
        string reverseAll = string.Concat(tokens.AsEnumerable().Reverse());
        string normalized = string.Join(' ', tokens);

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
