using GenMail.Core.Dedupe;
using GenMail.Core.Emailing;
using GenMail.Core.Generation;
using GenMail.Core.Models;
using GenMail.Core.Normalization;
using GenMail.Core.Numbering;
using GenMail.Core.Pipeline;
using GenMail.Core.Quality;
using GenMail.Core.Safety;
using Xunit;

namespace GenMail.Core.Tests;

public sealed class CoreFeatureTests
{
    [Fact]
    public void VietnameseAccentRemoval_Works()
    {
        DefaultNameNormalizer normalizer = new();
        NormalizedName name = normalizer.Normalize("Đặng Văn Lâm");
        Assert.Equal("dang van lam", name.Normalized);
    }

    [Theory]
    [InlineData("jdoe", true)]
    [InlineData("john.smith", true)]
    [InlineData("john_smith", true)]
    [InlineData("john-smith", true)]
    [InlineData("john@example.com", false)]
    [InlineData("http://example.com", false)]
    [InlineData("two words", false)]
    public void DirectUsernameDetector_Cases(string input, bool expected)
    {
        DefaultDirectUsernameDetector detector = new();
        Assert.Equal(expected, detector.IsDirectUsername(input));
    }

    [Fact]
    public void RuleCatalog_UniqueIds()
    {
        RuleCatalog catalog = new(BuiltInUsernameRules.Create());
        Assert.Equal(catalog.RuleIds.Count, catalog.RuleIds.Distinct().Count());
    }

    [Fact]
    public void TemplateRule_Renders()
    {
        TemplateUsernameRule rule = new("r", "{fi}.{last}");
        NormalizedName name = new("John Smith", "john smith", "john", string.Empty, "smith", "johnsmith", "smithjohn", false);
        Assert.Equal("j.smith", rule.Render(name));
    }

    [Fact]
    public void UsernameGenerator_RemovesDuplicates()
    {
        RuleCatalog catalog = new(new IUsernameRule[] { new TemplateUsernameRule("a", "{first}"), new TemplateUsernameRule("b", "{first}") });
        UsernameGenerator generator = new(catalog);
        NormalizedName name = new("John", "john", "john", string.Empty, "john", "john", "john", false);
        Assert.Single(generator.Generate(name, 1, new[] { "a", "b" }));
    }

    [Fact]
    public void NumberRangeParser_PaddedRanges()
    {
        NumberRangeParser parser = new();
        IReadOnlyList<string> values = parser.Parse("001-003,99");
        Assert.Equal(new[] { "001", "002", "003", "99" }, values);
    }

    [Fact]
    public void NumberExpansion_Works()
    {
        NumberExpansionService svc = new();
        IReadOnlyList<string> suffix = svc.Expand("john", new[] { "00", "01" }, NumberMode.NumberedOnly, NumberPlacementMode.SuffixOnly);
        Assert.Contains("john00", suffix);
        IReadOnlyList<string> infix = svc.Expand("john.smith", new[] { "99" }, NumberMode.NumberedOnly, NumberPlacementMode.InfixBeforeLastToken);
        Assert.Contains("john99.smith", infix);
    }

    [Fact]
    public void UsernameQualityPolicy_Rejections()
    {
        UsernameQualityPolicy policy = new();
        GenerationOptions options = GenerationOptions.Default;
        Assert.Equal(RejectionReason.RepeatedSeparators, policy.Validate("john..smith", options));
        Assert.Equal(RejectionReason.LooksLikeEmail, policy.Validate("john@example.com", options));
    }

    [Fact]
    public void EmailBuilder_ValidatesDomain()
    {
        EmailBuilder builder = new();
        Assert.Throws<ArgumentException>(() => builder.ValidateDomain("bad@domain"));
        Assert.Equal("john@example.com", builder.Build("john", "example.com"));
    }

    [Fact]
    public async Task InMemoryDedupe_SkipsDuplicate()
    {
        await using InMemoryDedupeStore store = new();
        Assert.True(await store.TryAddAsync(new DedupeEntry("s", "m", "k"), CancellationToken.None));
        Assert.False(await store.TryAddAsync(new DedupeEntry("s", "m", "k"), CancellationToken.None));
    }

    [Fact]
    public async Task SqliteDedupe_PersistsDuplicates()
    {
        string db = Path.Combine(Path.GetTempPath(), $"dedupe-{Guid.NewGuid():N}.db");
        await using (SqliteDedupeStore first = new(db))
        {
            Assert.True(await first.TryAddAsync(new DedupeEntry("s", "m", "k"), CancellationToken.None));
        }

        await using (SqliteDedupeStore second = new(db))
        {
            Assert.False(await second.TryAddAsync(new DedupeEntry("s", "m", "k"), CancellationToken.None));
        }
    }

    [Fact]
    public void SafetyGuard_RejectsHugeEstimate()
    {
        SafetyGuard guard = new();
        Assert.Throws<InvalidOperationException>(() => guard.EnsureSafe(new SafetyEstimate(1, 1, 2_000_000)));
    }

    [Fact]
    public async Task Pipeline_SmallIntegration()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"gm-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        string inputPath = Path.Combine(tempDir, "input.txt");
        await File.WriteAllLinesAsync(inputPath, new[] { "John Smith", "john.smith" });

        GenerationOptions options = GenerationOptions.Default with
        {
            Domain = "example.com",
            RuleIds = new[] { "first.last" },
            NumberMode = NumberMode.BaseOnly,
            OutputRootPath = tempDir
        };

        GenMailPipeline pipeline = new();
        ProcessingResult result = await pipeline.RunAsync(inputPath, options, progress: null, CancellationToken.None);
        Assert.True(File.Exists(Path.Combine(result.OutputDirectory, "emails.txt")));
    }
}
