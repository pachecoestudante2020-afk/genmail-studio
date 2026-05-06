using System.Runtime.CompilerServices;
using GenMail.Core.Models;

namespace GenMail.Core.IO;

public sealed class FastLineReader
{
    public async IAsyncEnumerable<InputRecord> ReadAsync(
        string path,
        bool skipEmptyLines,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using FileStream stream = new(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            useAsync: true);

        using StreamReader reader = new(stream);
        int lineNumber = 0;

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string? line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null)
            {
                continue;
            }

            lineNumber++;
            string trimmed = line.Trim();
            if (skipEmptyLines && trimmed.Length == 0)
            {
                continue;
            }

            yield return new InputRecord(lineNumber, line, trimmed);
        }
    }
}
