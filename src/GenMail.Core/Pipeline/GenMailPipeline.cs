using GenMail.Core.Dedupe;
using GenMail.Core.Emailing;
using GenMail.Core.Generation;
using GenMail.Core.IO;
using GenMail.Core.Models;
using GenMail.Core.Normalization;
using GenMail.Core.Numbering;
using GenMail.Core.Quality;
using GenMail.Core.Reports;
using GenMail.Core.Safety;

namespace GenMail.Core.Pipeline;

public sealed class GenMailPipeline
{
    private readonly FastLineReader _lineReader = new();
    private readonly INameNormalizer _normalizer = new DefaultNameNormalizer();
    private readonly IDirectUsernameDetector _directDetector = new DefaultDirectUsernameDetector();
    private readonly RuleCatalog _ruleCatalog = new(BuiltInUsernameRules.Create());
    private readonly NumberRangeParser _numberParser = new();
    private readonly NumberExpansionService _numberExpansion = new();
    private readonly UsernameQualityPolicy _qualityPolicy = new();
    private readonly EmailBuilder _emailBuilder = new();
    private readonly CsvReportWriter _csvReportWriter = new();
    private readonly SummaryWriter _summaryWriter = new();
    private readonly SafetyGuard _safetyGuard = new();

    public async Task<ProcessingResult> RunAsync(string inputPath, GenerationOptions options, IProgress<ProgressSnapshot>? progress, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(inputPath)) throw new ArgumentException("Input path is required.", nameof(inputPath));
        if (!File.Exists(inputPath)) throw new FileNotFoundException("Input file was not found.", inputPath);
        if (!string.Equals(Path.GetExtension(inputPath), ".txt", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Input file must be .txt.", nameof(inputPath));
        _emailBuilder.ValidateDomain(options.Domain);
        _safetyGuard.ValidateOptions(options);

        string outputDir = Path.Combine(options.OutputRootPath, DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"));
        Directory.CreateDirectory(outputDir);

        IReadOnlyList<string> activeRuleIds = options.RuleIds.Count == 0 ? _ruleCatalog.RuleIds.ToList() : options.RuleIds;
        UsernameGenerator generator = new(_ruleCatalog);
        IReadOnlyList<string> numbers = _numberParser.Parse(options.NumberPattern);

        await using IDedupeStore dedupe = options.DedupeMode switch
        {
            DedupeMode.Sqlite => new SqliteDedupeStore(Path.Combine(outputDir, "dedupe.db")),
            DedupeMode.InMemory => new InMemoryDedupeStore(),
            _ => new NoopDedupeStore()
        };

        long inputLines = File.ReadLines(inputPath).LongCount();
        SafetyEstimate estimate = new OutputEstimator(_numberParser).Estimate(inputLines, options);
        _safetyGuard.EnsureSafe(estimate);

        List<IReadOnlyList<string>> duplicateRows = new();
        List<IReadOnlyList<string>> qualityRows = new();
        List<IReadOnlyList<string>> rejectedInputRows = new();

        long linesProcessed = 0;
        long usernamesAccepted = 0;
        long qualityRejected = 0;
        long duplicateSkipped = 0;
        long rejectedInputs = 0;

        await using OutputFileSetWriter writer = new(outputDir, options.SplitOutputFiles, options.RowsPerOutputFile);

        await foreach (InputRecord record in _lineReader.ReadAsync(inputPath, options.SkipEmptyLines, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            linesProcessed++;
            string input = record.TrimmedText;

            NormalizedName normalized = _directDetector.IsDirectUsername(input)
                ? new NormalizedName(input, input.ToLowerInvariant(), input.ToLowerInvariant(), string.Empty, input.ToLowerInvariant(), input.ToLowerInvariant(), input.ToLowerInvariant(), true)
                : _normalizer.Normalize(input);

            if (normalized.All.Length == 0)
            {
                rejectedInputs++;
                rejectedInputRows.Add(new[] { record.LineNumber.ToString(), record.RawText, "empty_after_normalization" });
                continue;
            }

            IReadOnlyList<UsernameCandidate> candidates = generator.Generate(normalized, record.LineNumber, activeRuleIds);
            foreach (UsernameCandidate candidate in candidates)
            {
                IReadOnlyList<string> expanded = _numberExpansion.Expand(candidate.Username, numbers, options.NumberMode, options.NumberPlacementMode);
                foreach (string username in expanded)
                {
                    RejectionReason reason = _qualityPolicy.Validate(username, options);
                    if (reason != RejectionReason.None)
                    {
                        qualityRejected++;
                        qualityRows.Add(new[] { record.LineNumber.ToString(), username, reason.ToString() });
                        continue;
                    }

                    bool added = await dedupe.TryAddAsync(new DedupeEntry("global", "username", username), cancellationToken).ConfigureAwait(false);
                    if (!added)
                    {
                        duplicateSkipped++;
                        duplicateRows.Add(new[] { record.LineNumber.ToString(), username });
                        continue;
                    }

                    string email = _emailBuilder.Build(username, options.Domain);
                    await writer.WriteAsync(username, email).ConfigureAwait(false);
                    usernamesAccepted++;
                }
            }

            if (linesProcessed % 100 == 0)
            {
                progress?.Report(new ProgressSnapshot(linesProcessed, usernamesAccepted, usernamesAccepted, "processing", DateTimeOffset.UtcNow));
            }
        }

        ProcessingCounters counters = new(linesProcessed, rejectedInputs, usernamesAccepted, qualityRejected, duplicateSkipped, usernamesAccepted, writer.FilesCreated.Count, options.RowsPerOutputFile);
        List<string> generatedFiles = new(writer.FilesCreated)
        {
            Path.Combine(outputDir, "duplicate_skipped.csv"),
            Path.Combine(outputDir, "quality_rejected.csv"),
            Path.Combine(outputDir, "rejected_inputs.csv"),
            Path.Combine(outputDir, "summary.txt")
        };
        ProcessingResult result = new(outputDir, counters, estimate, generatedFiles);

        await _csvReportWriter.WriteRowsAsync(Path.Combine(outputDir, "duplicate_skipped.csv"), new[] { "line", "username" }, duplicateRows, cancellationToken).ConfigureAwait(false);
        await _csvReportWriter.WriteRowsAsync(Path.Combine(outputDir, "quality_rejected.csv"), new[] { "line", "username", "reason" }, qualityRows, cancellationToken).ConfigureAwait(false);
        await _csvReportWriter.WriteRowsAsync(Path.Combine(outputDir, "rejected_inputs.csv"), new[] { "line", "input", "reason" }, rejectedInputRows, cancellationToken).ConfigureAwait(false);
        await _summaryWriter.WriteAsync(Path.Combine(outputDir, "summary.txt"), result, options, cancellationToken).ConfigureAwait(false);

        progress?.Report(new ProgressSnapshot(linesProcessed, usernamesAccepted, usernamesAccepted, "completed", DateTimeOffset.UtcNow));
        return result;
    }
}
