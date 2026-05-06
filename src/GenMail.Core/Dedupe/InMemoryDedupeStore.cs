using GenMail.Core.Models;

namespace GenMail.Core.Dedupe;

public sealed class InMemoryDedupeStore : IDedupeStore
{
    private readonly HashSet<string> _keys = new(StringComparer.Ordinal);

    public ValueTask<bool> TryAddAsync(DedupeEntry entry, CancellationToken cancellationToken)
    {
        string composite = $"{entry.Scope}|{entry.KeyMode}|{entry.DedupeKey}";
        return ValueTask.FromResult(_keys.Add(composite));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
