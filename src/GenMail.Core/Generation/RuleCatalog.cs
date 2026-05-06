using GenMail.Core.Models;

namespace GenMail.Core.Generation;

public sealed class RuleCatalog
{
    private readonly Dictionary<string, IUsernameRule> _rules;

    public RuleCatalog(IEnumerable<IUsernameRule> rules)
    {
        _rules = new Dictionary<string, IUsernameRule>(StringComparer.Ordinal);
        foreach (IUsernameRule rule in rules)
        {
            if (!_rules.TryAdd(rule.Id, rule))
            {
                throw new InvalidOperationException($"Duplicate rule id: {rule.Id}");
            }
        }
    }

    public IReadOnlyCollection<string> RuleIds => _rules.Keys;

    public IUsernameRule Get(string id) => _rules[id];

    public IEnumerable<IUsernameRule> Select(IEnumerable<string> ids)
    {
        foreach (string id in ids)
        {
            if (_rules.TryGetValue(id, out IUsernameRule? rule))
            {
                yield return rule;
            }
        }
    }
}
