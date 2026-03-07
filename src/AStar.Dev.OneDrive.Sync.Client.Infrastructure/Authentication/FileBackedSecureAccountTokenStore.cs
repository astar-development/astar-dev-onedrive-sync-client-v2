using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AStar.Dev.Functional.Extensions;
using AStar.Dev.Utilities;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Authentication;

/// <summary>
/// Provides encrypted file-backed storage for account tokens.
/// </summary>
public sealed class FileBackedSecureAccountTokenStore : ISecureAccountTokenStore
{
    private const string TokenFileName = "account-tokens.json";
    private const string KeyFileName = "account-tokens.key";
    private readonly string _rootPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileBackedSecureAccountTokenStore"/> class.
    /// </summary>
    /// <param name="rootPath">Optional storage root path.</param>
    public FileBackedSecureAccountTokenStore(string? rootPath = null) => _rootPath = rootPath ?? Path.Combine(Path.GetDirectoryName(Data.DatabasePathResolver.ResolveDatabasePath())!, "secure-store");

    /// <inheritdoc />
    public async Task<Result<Unit, string>> SaveAsync(string accountId, SecureAccountTokens tokens, CancellationToken cancellationToken = default)
        => await Try.RunAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, string> encrypted = await LoadEncryptedMapAsync(cancellationToken);
            encrypted[accountId] = Encrypt(JsonSerializer.Serialize(tokens));
            await PersistEncryptedMapAsync(encrypted, cancellationToken);
            return Unit.Value;
        }).MapFailureAsync(error => error.Message);

    /// <inheritdoc />
    public async Task<Result<Option<SecureAccountTokens>, string>> LoadAsync(string accountId, CancellationToken cancellationToken = default)
        => await Try.RunAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, string> encrypted = await LoadEncryptedMapAsync(cancellationToken);
            if(!encrypted.TryGetValue(accountId, out var cipherText))
            {
                return Option.None<SecureAccountTokens>();
            }

            var json = Decrypt(cipherText);
            SecureAccountTokens? tokens = JsonSerializer.Deserialize<SecureAccountTokens>(json);
            return tokens is null
                ? Option.None<SecureAccountTokens>()
                : new Option<SecureAccountTokens>.Some(tokens);
        }).MapFailureAsync(error => error.Message);

    /// <inheritdoc />
    public async Task<Result<Unit, string>> RemoveAsync(string accountId, CancellationToken cancellationToken = default)
        => await Try.RunAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, string> encrypted = await LoadEncryptedMapAsync(cancellationToken);
            _ = encrypted.Remove(accountId);
            await PersistEncryptedMapAsync(encrypted, cancellationToken);
            return Unit.Value;
        }).MapFailureAsync(error => error.Message);

    private async Task<Dictionary<string, string>> LoadEncryptedMapAsync(CancellationToken cancellationToken)
    {
        _ = Directory.CreateDirectory(_rootPath);
        var path = _rootPath.CombinePath(TokenFileName);
        if(!File.Exists(path))
        {
            return [];
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return string.IsNullOrWhiteSpace(json) ? [] : JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
    }

    private async Task PersistEncryptedMapAsync(Dictionary<string, string> encrypted, CancellationToken cancellationToken)
    {
        _ = Directory.CreateDirectory(_rootPath);
        var path = _rootPath.CombinePath(TokenFileName);
        var json = JsonSerializer.Serialize(encrypted);
        await File.WriteAllTextAsync(path, json, cancellationToken);
    }

    private string Encrypt(string plainText)
    {
        var key = GetOrCreateKey();
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(key, 16);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        byte[] payload = [.. nonce, .. tag, .. cipherBytes];
        return Convert.ToBase64String(payload);
    }

    private string Decrypt(string cipherText)
    {
        var payload = Convert.FromBase64String(cipherText);
        var nonce = payload[..12];
        var tag = payload[12..28];
        var cipherBytes = payload[28..];
        var plainBytes = new byte[cipherBytes.Length];
        var key = GetOrCreateKey();
        using var aes = new AesGcm(key, 16);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
        return Encoding.UTF8.GetString(plainBytes);
    }

    private byte[] GetOrCreateKey()
    {
        _ = Directory.CreateDirectory(_rootPath);
        var keyPath = _rootPath.CombinePath(KeyFileName);
        if(File.Exists(keyPath))
        {
            return File.ReadAllBytes(keyPath);
        }

        var key = RandomNumberGenerator.GetBytes(32);
        File.WriteAllBytes(keyPath, key);
        return key;
    }
}
