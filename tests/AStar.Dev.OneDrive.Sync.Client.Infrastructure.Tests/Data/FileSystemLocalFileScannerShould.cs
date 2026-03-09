using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;
using AStar.Dev.Utilities;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Tests.Data;

public sealed class FileSystemLocalFileScannerShould
{
    [Fact]
    public async Task ProduceDeterministicOrderedSnapshot()
    {
        var rootPath = Path.GetTempPath().CombinePath($"astar-local-scan-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootPath);
        var pathB = rootPath.CombinePath("b.txt");
        var pathA = rootPath.CombinePath("a.txt");
        await File.WriteAllTextAsync(pathB, "bbb", TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(pathA, "aaa", TestContext.Current.CancellationToken);
        DateTime timestamp = new(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(pathA, timestamp);
        File.SetLastWriteTimeUtc(pathB, timestamp);
        FileSystemLocalFileScanner scanner = new();

        Result<IReadOnlyList<LocalInventoryItem>, string> firstResult = await scanner.ScanAsync(rootPath, TestContext.Current.CancellationToken);
        Result<IReadOnlyList<LocalInventoryItem>, string> secondResult = await scanner.ScanAsync(rootPath, TestContext.Current.CancellationToken);
        IReadOnlyList<LocalInventoryItem> first = firstResult.ShouldBeOfType<Result<IReadOnlyList<LocalInventoryItem>, string>.Ok>().Value;
        IReadOnlyList<LocalInventoryItem> second = secondResult.ShouldBeOfType<Result<IReadOnlyList<LocalInventoryItem>, string>.Ok>().Value;
        first.Select(x => x.RelativePath).ToArray().ShouldBe(["a.txt", "b.txt"]);
        first.Select(x => x.Fingerprint).ToArray().ShouldBe(second.Select(x => x.Fingerprint).ToArray());
    }
}
