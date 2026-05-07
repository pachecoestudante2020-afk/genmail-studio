using GenMail.Core.Models;

namespace GenMail.Core.Safety;

public sealed class SafetyGuard
{
    public long MaxOutputEmails { get; init; } = 1_000_000;
    public long MaxNumbersPerBase { get; init; } = 1_000;
    public long MaxInputLinesBeforeWarning { get; init; } = 500_000;
    public int MinRowsPerOutputFile { get; init; } = 1;
    public int MaxRowsPerOutputFile { get; init; } = 10_000_000;

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

    public void ValidateOptions(GenerationOptions options)
    {
        if (!options.SplitOutputFiles)
        {
            return;
        }

        if (!options.RowsPerOutputFile.HasValue)
        {
            throw new InvalidOperationException("RowsPerOutputFile is required when SplitOutputFiles is enabled.");
        }

        int rows = options.RowsPerOutputFile.Value;
        if (rows < MinRowsPerOutputFile || rows > MaxRowsPerOutputFile)
        {
            throw new InvalidOperationException($"RowsPerOutputFile must be between {MinRowsPerOutputFile} and {MaxRowsPerOutputFile}.");
        }
    }
}
