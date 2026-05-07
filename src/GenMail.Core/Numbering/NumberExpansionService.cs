using GenMail.Core.Models;

namespace GenMail.Core.Numbering;

public sealed class NumberExpansionService
{
    public IReadOnlyList<string> Expand(string baseUsername, IReadOnlyList<string> numbers, NumberMode numberMode, NumberPlacementMode placementMode)
    {
        HashSet<string> set = new(StringComparer.Ordinal);
        if (numberMode is NumberMode.BaseOnly or NumberMode.BaseAndNumbered)
        {
            set.Add(baseUsername);
        }

        if (numberMode is NumberMode.NumberedOnly or NumberMode.BaseAndNumbered)
        {
            foreach (string number in numbers)
            {
                foreach (string value in Place(baseUsername, number, placementMode))
                {
                    set.Add(value);
                }
            }
        }

        return set.ToList();
    }

    private static IEnumerable<string> Place(string baseUsername, string number, NumberPlacementMode mode)
    {
        if (mode is NumberPlacementMode.SuffixOnly or NumberPlacementMode.SuffixAndPrefix or NumberPlacementMode.All)
        {
            yield return baseUsername + number;
        }

        if (mode is NumberPlacementMode.PrefixOnly or NumberPlacementMode.SuffixAndPrefix or NumberPlacementMode.All)
        {
            yield return number + baseUsername;
        }

        if (mode is NumberPlacementMode.InfixBeforeLastToken or NumberPlacementMode.All)
        {
            int separatorIndex = baseUsername.LastIndexOfAny(['.', '_', '-']);
            if (separatorIndex > 0)
            {
                yield return baseUsername[..separatorIndex] + number + baseUsername[separatorIndex..];
            }
            else
            {
                yield return baseUsername + number;
            }
        }
    }
}
