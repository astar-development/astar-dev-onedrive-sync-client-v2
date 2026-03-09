namespace AStar.Dev.OneDrive.Sync.Client.Application.Models;

/// <summary>
/// Represents a local file snapshot entry used for sync comparison.
/// </summary>
/// <param name="LocalPath">The absolute local file path.</param>
/// <param name="RelativePath">The file path relative to the scanned root.</param>
/// <param name="SizeBytes">The file size in bytes.</param>
/// <param name="LastWriteUtc">The file last-write timestamp in UTC.</param>
/// <param name="Fingerprint">The deterministic fingerprint value.</param>
/// <param name="FingerprintPolicy">The fingerprint policy identifier.</param>
public sealed record LocalInventoryItem(
    string LocalPath,
    string RelativePath,
    long SizeBytes,
    DateTime LastWriteUtc,
    string Fingerprint,
    string FingerprintPolicy);