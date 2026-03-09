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
        var timestamp = new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(pathA, timestamp);
        File.SetLastWriteTimeUtc(pathB, timestamp);
        var scanner = new FileSystemLocalFileScanner();

        var firstResult = await scanner.ScanAsync(rootPath, TestContext.Current.CancellationToken);
        var secondResult = await scanner.ScanAsync(rootPath, TestContext.Current.CancellationToken);

        var first = firstResult.ShouldBeOfType<AStar.Dev.Functional.Extensions.Result<IReadOnlyList<AStar.Dev.OneDrive.Sync.Client.Application.Models.LocalInventoryItem>, string>.Ok>().Value;
        var second = secondResult.ShouldBeOfType<AStar.Dev.Functional.Extensions.Result<IReadOnlyList<AStar.Dev.OneDrive.Sync.Client.Application.Models.LocalInventoryItem>, string>.Ok>().Value;
        first.Select(x => x.RelativePath).ToArray().ShouldBe(["a.txt", "b.txt"]);
        first.Select(x => x.Fingerprint).ToArray().ShouldBe(second.Select(x => x.Fingerprint).ToArray());
    }
}