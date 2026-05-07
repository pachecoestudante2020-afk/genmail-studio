using System.Text.RegularExpressions;
using GenMail.Core.Models;

namespace GenMail.Core.Quality;

public sealed partial class UsernameQualityPolicy
{
    [GeneratedRegex("^[a-z0-9._-]+$")]
    private static partial Regex AllowedRegex();

    [GeneratedRegex("[._-]{2,}")]
    private static partial Regex RepeatedSeparatorRegex();

    public RejectionReason Validate(string username, GenerationOptions options)
    {
        if (string.IsNullOrWhiteSpace(username)) return RejectionReason.Empty;
        if (username.Length < options.MinUsernameLength) return RejectionReason.TooShort;
        if (username.Length > options.MaxUsernameLength) return RejectionReason.TooLong;
        if (username.Contains('@')) return RejectionReason.LooksLikeEmail;
        if (username.Contains("://", StringComparison.Ordinal)) return RejectionReason.LooksLikeUrl;
        if (!AllowedRegex().IsMatch(username)) return RejectionReason.InvalidCharacters;
        if (RepeatedSeparatorRegex().IsMatch(username)) return RejectionReason.RepeatedSeparators;
        if ("._-".Contains(username[0]) || "._-".Contains(username[^1])) return RejectionReason.LeadingOrTrailingSeparator;
        if (!options.AllowAllDigitUsernames && username.All(char.IsDigit)) return RejectionReason.AllDigitsNotAllowed;
        return RejectionReason.None;
    }
}
