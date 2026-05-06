using GenMail.Core.Models;

namespace GenMail.Core.Safety;

public sealed class SafetyGuard
{
    public long MaxOutputEmails { get; init; } = 1_000_000;
    public long MaxNumbersPerBase { get; init; } = 1_000;
    public long MaxInputLinesBeforeWarning { get; init; } = 500_000;

    public void EnsureSafe(SafetyEstimate estimate)
    {
        if (estimate.EstimatedOutputs > MaxOutputEmails)
        {
            throw new InvalidOperationException("Estimated output exceeds safety limit.");
        }

        if (estimate.EstimatedNumbersPerBase > MaxNumbersPerBase)
        {
            throw new InvalidOperationException("Numbers per base exceeds safety limit.");
        }
    }
}
