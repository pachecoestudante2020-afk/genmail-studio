using GenMail.Core.Models;
using Microsoft.Data.Sqlite;

namespace GenMail.Core.Dedupe;

public sealed class SqliteDedupeStore : IDedupeStore
{
    private readonly SqliteConnection _connection;

    public SqliteDedupeStore(string path)
    {
        _connection = new SqliteConnection($"Data Source={path}");
        _connection.Open();
        using SqliteCommand pragmaWal = _connection.CreateCommand();
        pragmaWal.CommandText = "PRAGMA journal_mode=WAL;";
        pragmaWal.ExecuteNonQuery();
        using SqliteCommand pragmaSync = _connection.CreateCommand();
        pragmaSync.CommandText = "PRAGMA synchronous=NORMAL;";
        pragmaSync.ExecuteNonQuery();
        using SqliteCommand create = _connection.CreateCommand();
        create.CommandText = "CREATE TABLE IF NOT EXISTS generated_keys (scope TEXT NOT NULL, key_mode TEXT NOT NULL, dedupe_key TEXT NOT NULL, PRIMARY KEY(scope, key_mode, dedupe_key));";
        create.ExecuteNonQuery();
    }

    public async ValueTask<bool> TryAddAsync(DedupeEntry entry, CancellationToken cancellationToken)
    {
        using SqliteCommand insert = _connection.CreateCommand();
        insert.CommandText = "INSERT OR IGNORE INTO generated_keys(scope,key_mode,dedupe_key) VALUES($scope,$mode,$key);";
        insert.Parameters.AddWithValue("$scope", entry.Scope);
        insert.Parameters.AddWithValue("$mode", entry.KeyMode);
        insert.Parameters.AddWithValue("$key", entry.DedupeKey);
        int affected = await insert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return affected > 0;
    }

    public ValueTask DisposeAsync()
    {
        _connection.Dispose();
        return ValueTask.CompletedTask;
    }
}
