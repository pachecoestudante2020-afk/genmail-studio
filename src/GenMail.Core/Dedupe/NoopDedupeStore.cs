using GenMail.Core.Models;

namespace GenMail.Core.Dedupe;

public sealed class NoopDedupeStore : IDedupeStore
{
    public ValueTask<bool> TryAddAsync(DedupeEntry entry, CancellationToken cancellationToken) => ValueTask.FromResult(true);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
