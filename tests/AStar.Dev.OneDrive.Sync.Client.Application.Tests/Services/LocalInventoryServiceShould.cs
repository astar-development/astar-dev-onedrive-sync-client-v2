using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Application.Services;
using NSubstitute;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Tests.Services;

public sealed class LocalInventoryServiceShould
{
    private readonly ILocalFileScanner _scanner;
    private readonly ILocalInventoryStore _store;
    private readonly ILocalInventoryService _sut;

    public LocalInventoryServiceShould()
    {
        _scanner = Substitute.For<ILocalFileScanner>();
        _store = Substitute.For<ILocalInventoryStore>();
        _sut = new LocalInventoryService(_scanner, _store);
    }

    [Fact]
    public async Task PersistSnapshotWhenManualScanRuns()
    {
        var accountId = "acct-1";
        var rootPath = "/tmp/sync";
        IReadOnlyList<LocalInventoryItem> snapshot =
        [
            new("/tmp/sync/a.txt", "a.txt", 10, DateTime.UnixEpoch, "fp-a", "metadata-sha256-v1")
        ];
        Result<IReadOnlyList<LocalInventoryItem>, string> scanResult = new List<LocalInventoryItem>(snapshot);
        _ = _scanner.ScanAsync(rootPath, Arg.Any<CancellationToken>()).Returns(Task.FromResult(scanResult));
        _ = _store.SaveAsync(accountId, Arg.Any<IReadOnlyList<LocalInventoryItem>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));

        Result<IReadOnlyList<LocalInventoryItem>, string> result = await _sut.RunManualScanAsync(accountId, rootPath, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<Result<IReadOnlyList<LocalInventoryItem>, string>.Ok>().Value.ShouldBe(snapshot);
        await _store.Received(1).SaveAsync(accountId, Arg.Any<IReadOnlyList<LocalInventoryItem>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PersistSnapshotWhenStartupScanRuns()
    {
        var accountId = "acct-2";
        var rootPath = "/tmp/sync";
        IReadOnlyList<LocalInventoryItem> snapshot =
        [
            new("/tmp/sync/b.txt", "b.txt", 20, DateTime.UnixEpoch, "fp-b", "metadata-sha256-v1")
        ];
        Result<IReadOnlyList<LocalInventoryItem>, string> scanResult = new List<LocalInventoryItem>(snapshot);
        _ = _scanner.ScanAsync(rootPath, Arg.Any<CancellationToken>()).Returns(Task.FromResult(scanResult));
        _ = _store.SaveAsync(accountId, Arg.Any<IReadOnlyList<LocalInventoryItem>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, string>>(Unit.Value));

        Result<IReadOnlyList<LocalInventoryItem>, string> result = await _sut.RunStartupScanAsync(accountId, rootPath, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<Result<IReadOnlyList<LocalInventoryItem>, string>.Ok>().Value.ShouldBe(snapshot);
        await _store.Received(1).SaveAsync(accountId, Arg.Any<IReadOnlyList<LocalInventoryItem>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReturnErrorWhenScannerFails()
    {
        _ = _scanner.ScanAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LocalInventoryItem>, string>>("scan failed"));

        Result<IReadOnlyList<LocalInventoryItem>, string> result = await _sut.RunManualScanAsync("acct-3", "/tmp", TestContext.Current.CancellationToken);

        result.ShouldBeOfType<Result<IReadOnlyList<LocalInventoryItem>, string>.Error>().Reason.ShouldBe("scan failed");
        await _store.DidNotReceive().SaveAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<LocalInventoryItem>>(), Arg.Any<CancellationToken>());
    }
}