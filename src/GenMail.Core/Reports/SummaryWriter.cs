using GenMail.Core.Models;

namespace GenMail.Core.Reports;

public sealed class SummaryWriter
{
    public async Task WriteAsync(string path, ProcessingResult result, CancellationToken cancellationToken)
    {
        List<string> lines =
        [
            $"OutputDirectory: {result.OutputDirectory}",
            $"TotalLines: {result.Counters.TotalLines}",
            $"RejectedInputs: {result.Counters.RejectedInputs}",
            $"UsernamesGenerated: {result.Counters.UsernamesGenerated}",
            $"QualityRejected: {result.Counters.QualityRejected}",
            $"DuplicateSkipped: {result.Counters.DuplicateSkipped}",
            $"EmailsWritten: {result.Counters.EmailsWritten}",
            $"EstimatedOutputs: {result.SafetyEstimate.EstimatedOutputs}"
        ];

        await File.WriteAllLinesAsync(path, lines, cancellationToken).ConfigureAwait(false);
    }
}
