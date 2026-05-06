namespace GenMail.Core.Numbering;

public sealed class NumberRangeParser
{
    public IReadOnlyList<string> Parse(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return Array.Empty<string>();
        }

        List<string> results = new();
        foreach (string part in expression.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (part.Contains('-', StringComparison.Ordinal))
            {
                string[] bounds = part.Split('-', 2);
                int start = int.Parse(bounds[0]);
                int end = int.Parse(bounds[1]);
                int width = Math.Max(bounds[0].Length, bounds[1].Length);
                for (int i = start; i <= end; i++)
                {
                    results.Add(i.ToString().PadLeft(width, '0'));
                }
            }
            else
            {
                results.Add(part);
            }
        }

        return results;
    }
}
