using System.Text.RegularExpressions;

namespace GenMail.Core.Normalization;

public sealed partial class DefaultDirectUsernameDetector : IDirectUsernameDetector
{
    [GeneratedRegex("^[a-zA-Z0-9._-]+$")]
    private static partial Regex UsernameRegex();

    public bool IsDirectUsername(string input)
    {
        string value = input.Trim();
        if (value.Contains(' ') || value.Contains('@') || value.Contains("://", StringComparison.Ordinal))
        {
            return false;
        }

        return UsernameRegex().IsMatch(value);
    }
}
