namespace GenMail.Core.Models;

public sealed record InputRecord(int LineNumber, string RawText, string TrimmedText);

public sealed record NormalizedName(
    string OriginalInput,
    string Normalized,
    string First,
    string Middle,
    string Last,
    string All,
    string ReverseAll,
    bool IsDirectUsername);

public sealed record UsernameCandidate(string Username, string RuleId, int InputLineNumber);

public sealed record EmailCandidate(string Username, string Email, int InputLineNumber);

public sealed record ProcessingCounters(
    long TotalLines,
    long RejectedInputs,
    long UsernamesGenerated,
    long QualityRejected,
    long DuplicateSkipped,
    long EmailsWritten);

public sealed record ProcessingResult(
    string OutputDirectory,
    ProcessingCounters Counters,
    SafetyEstimate SafetyEstimate,
    IReadOnlyList<string> GeneratedFiles);

public sealed record ProgressSnapshot(
    long LinesProcessed,
    long UsernamesAccepted,
    long EmailsWritten,
    string Stage,
    DateTimeOffset Timestamp);

public sealed record SafetyEstimate(long EstimatedInputLines, long EstimatedNumbersPerBase, long EstimatedOutputs);

public sealed record UsernameRuleDefinition(string Id, string Template, string Description);

public sealed record DedupeEntry(string Scope, string KeyMode, string DedupeKey);
