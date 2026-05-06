using GenMail.Core.Models;

namespace GenMail.Core.Dedupe;

public interface IDedupeStore : IAsyncDisposable
{
    ValueTask<bool> TryAddAsync(DedupeEntry entry, CancellationToken cancellationToken);
}
