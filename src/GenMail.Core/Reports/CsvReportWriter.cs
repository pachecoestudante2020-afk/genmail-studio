namespace GenMail.Core.Reports;

public sealed class CsvReportWriter
{
    public async Task WriteRowsAsync(string path, IReadOnlyList<string> header, IEnumerable<IReadOnlyList<string>> rows, CancellationToken cancellationToken)
    {
        await using StreamWriter writer = new(path, false);
        await writer.WriteLineAsync(string.Join(',', header.Select(Escape))).ConfigureAwait(false);
        foreach (IReadOnlyList<string> row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await writer.WriteLineAsync(string.Join(',', row.Select(Escape))).ConfigureAwait(false);
        }
    }

    private static string Escape(string value) =>
        value.Contains(',') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}
