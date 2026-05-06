namespace GenMail.Core.Models;

public sealed record GenerationOptions(
    string Domain,
    IReadOnlyList<string> RuleIds,
    string NumberPattern,
    NumberMode NumberMode,
    NumberPlacementMode NumberPlacementMode,
    DedupeMode DedupeMode,
    AliasFilterMode AliasFilterMode,
    bool SkipEmptyLines,
    bool AllowAllDigitUsernames,
    int MinUsernameLength,
    int MaxUsernameLength,
    string OutputRootPath)
{
    public static GenerationOptions Default => new(
        Domain: "example.com",
        RuleIds: Array.Empty<string>(),
        NumberPattern: string.Empty,
        NumberMode: NumberMode.BaseOnly,
        NumberPlacementMode: NumberPlacementMode.SuffixOnly,
        DedupeMode: DedupeMode.InMemory,
        AliasFilterMode: AliasFilterMode.None,
        SkipEmptyLines: true,
        AllowAllDigitUsernames: false,
        MinUsernameLength: 3,
        MaxUsernameLength: 32,
        OutputRootPath: "output");
}
