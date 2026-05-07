using GenMail.Core.Models;

namespace GenMail.Core.Reports;

public sealed class SummaryWriter
{
    public async Task WriteAsync(string path, ProcessingResult result, GenerationOptions options, CancellationToken cancellationToken)
    {
        List<string> lines =
        [
            $"output_directory: {result.OutputDirectory}",
            $"total_lines: {result.Counters.TotalLines}",
            $"rejected_inputs: {result.Counters.RejectedInputs}",
            $"usernames_generated: {result.Counters.UsernamesGenerated}",
            $"quality_rejected: {result.Counters.QualityRejected}",
            $"duplicate_skipped: {result.Counters.DuplicateSkipped}",
            $"emails_written: {result.Counters.EmailsWritten}",
            $"split_output_files: {options.SplitOutputFiles.ToString().ToLowerInvariant()}",
            $"rows_per_output_file: {(options.RowsPerOutputFile?.ToString() ?? "null")}",
            $"output_files_created: {result.Counters.OutputFilesCreated}",
            $"total_emails_written: {result.Counters.EmailsWritten}",
            $"total_usernames_written: {result.Counters.UsernamesGenerated}",
            $"estimated_outputs: {result.SafetyEstimate.EstimatedOutputs}"
        ];

        await File.WriteAllLinesAsync(path, lines, cancellationToken).ConfigureAwait(false);
    }
}
