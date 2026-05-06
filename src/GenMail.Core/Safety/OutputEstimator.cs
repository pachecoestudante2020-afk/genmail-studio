using GenMail.Core.Models;
using GenMail.Core.Numbering;

namespace GenMail.Core.Safety;

public sealed class OutputEstimator(NumberRangeParser parser)
{
    public SafetyEstimate Estimate(long inputLines, GenerationOptions options)
    {
        int numberCount = parser.Parse(options.NumberPattern).Count;
        long estimatedPerBase = options.NumberMode == NumberMode.BaseOnly ? 1 : Math.Max(1, numberCount);
        long outputs = inputLines * estimatedPerBase * Math.Max(1, options.RuleIds.Count);
        return new SafetyEstimate(inputLines, estimatedPerBase, outputs);
    }
}
