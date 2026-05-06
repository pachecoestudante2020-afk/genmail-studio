using System.Text.RegularExpressions;

namespace GenMail.Core.Emailing;

public sealed partial class EmailBuilder
{
    [GeneratedRegex("^[a-zA-Z0-9-]+(\\.[a-zA-Z0-9-]+)+$")]
    private static partial Regex DomainRegex();

    public string Build(string username, string domain)
    {
        ValidateDomain(domain);
        return $"{username}@{domain.ToLowerInvariant()}";
    }

    public void ValidateDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain)) throw new ArgumentException("Domain cannot be empty.", nameof(domain));
        if (domain.Contains('@')) throw new ArgumentException("Domain cannot contain '@'.", nameof(domain));
        if (!DomainRegex().IsMatch(domain)) throw new ArgumentException("Domain format is invalid.", nameof(domain));
    }
}
