using System.Text;

namespace GenMail.Core.Pipeline;

public sealed class OutputFileSetWriter : IAsyncDisposable
{
    private readonly string _outputDir;
    private readonly bool _split;
    private readonly int? _rowsPerFile;
    private readonly List<string> _files = new();
    private StreamWriter? _usernameWriter;
    private StreamWriter? _emailWriter;
    private int _currentFileIndex;
    private int _rowsInCurrent;

    public OutputFileSetWriter(string outputDir, bool split, int? rowsPerFile)
    {
        _outputDir = outputDir;
        _split = split;
        _rowsPerFile = rowsPerFile;
    }

    public IReadOnlyList<string> FilesCreated => _files;

    public int FilePairsCreated => _currentFileIndex;

    public async Task WriteAsync(string username, string email)
    {
        if (_usernameWriter is null || _emailWriter is null)
        {
            await OpenNextAsync().ConfigureAwait(false);
        }

        if (_split && _rowsPerFile.HasValue && _rowsInCurrent >= _rowsPerFile.Value)
        {
            await RotateAsync().ConfigureAwait(false);
        }

        await _usernameWriter!.WriteLineAsync(username).ConfigureAwait(false);
        await _emailWriter!.WriteLineAsync(email).ConfigureAwait(false);
        _rowsInCurrent++;
    }

    private async Task RotateAsync()
    {
        if (_usernameWriter is not null)
        {
            await _usernameWriter.DisposeAsync().ConfigureAwait(false);
        }

        if (_emailWriter is not null)
        {
            await _emailWriter.DisposeAsync().ConfigureAwait(false);
        }

        await OpenNextAsync().ConfigureAwait(false);
    }

    private Task OpenNextAsync()
    {
        _currentFileIndex++;
        _rowsInCurrent = 0;

        string usernamesPath;
        string emailsPath;
        if (_split)
        {
            string suffix = _currentFileIndex.ToString("000");
            usernamesPath = Path.Combine(_outputDir, $"usernames_{suffix}.txt");
            emailsPath = Path.Combine(_outputDir, $"emails_{suffix}.txt");
        }
        else
        {
            usernamesPath = Path.Combine(_outputDir, "usernames.txt");
            emailsPath = Path.Combine(_outputDir, "emails.txt");
        }

        _usernameWriter = new StreamWriter(usernamesPath, false, new UTF8Encoding(false));
        _emailWriter = new StreamWriter(emailsPath, false, new UTF8Encoding(false));
        _files.Add(usernamesPath);
        _files.Add(emailsPath);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_usernameWriter is not null)
        {
            await _usernameWriter.DisposeAsync().ConfigureAwait(false);
        }

        if (_emailWriter is not null)
        {
            await _emailWriter.DisposeAsync().ConfigureAwait(false);
        }
    }
}
