using GenMail.Core.Models;

namespace GenMail.Core.Generation;

public sealed class TemplateUsernameRule(string id, string template) : IUsernameRule
{
    public string Id { get; } = id;

    public string Render(NormalizedName name)
    {
        Dictionary<string, string> values = new(StringComparer.Ordinal)
        {
            ["{first}"] = name.First,
            ["{last}"] = name.Last,
            ["{middle}"] = name.Middle,
            ["{all}"] = name.All,
            ["{reverseAll}"] = name.ReverseAll,
            ["{fi}"] = Take(name.First, 1),
            ["{li}"] = Take(name.Last, 1),
            ["{mi}"] = Take(name.Middle, 1),
            ["{rmi}"] = Take(name.Middle, 1),
            ["{first2}"] = Take(name.First, 2),
            ["{last2}"] = Take(name.Last, 2),
            ["{first3}"] = Take(name.First, 3),
            ["{last3}"] = Take(name.Last, 3),
            ["{first4}"] = Take(name.First, 4),
            ["{last4}"] = Take(name.Last, 4)
        };

        string result = template;
        foreach (KeyValuePair<string, string> kvp in values)
        {
            result = result.Replace(kvp.Key, kvp.Value, StringComparison.Ordinal);
        }

        return result;
    }

    private static string Take(string value, int length) =>
        value.Length <= length ? value : value[..length];
}
