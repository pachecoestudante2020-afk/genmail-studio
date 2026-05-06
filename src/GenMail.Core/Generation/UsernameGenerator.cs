using GenMail.Core.Models;

namespace GenMail.Core.Generation;

public sealed class UsernameGenerator(RuleCatalog catalog)
{
    public IReadOnlyList<UsernameCandidate> Generate(NormalizedName name, int lineNumber, IReadOnlyList<string> ruleIds)
    {
        HashSet<string> seen = new(StringComparer.Ordinal);
        List<UsernameCandidate> results = new();

        foreach (IUsernameRule rule in catalog.Select(ruleIds))
        {
            string value = rule.Render(name);
            if (seen.Add(value))
            {
                results.Add(new UsernameCandidate(value, rule.Id, lineNumber));
            }
        }

        return results;
    }
}
