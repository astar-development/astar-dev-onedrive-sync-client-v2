using AStar.Dev.Functional.Extensions;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Application.Services;
using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;
using AStar.Dev.OneDrive.Sync.Client.Domain.Interfaces;
using NSubstitute;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Tests.Services;

public sealed class SyncServiceShould
{
    private readonly ISyncFileRepository _repository;
    private readonly ISyncService _sut;

    public SyncServiceShould()
    {
        _repository = Substitute.For<ISyncFileRepository>();
        _sut = new SyncService(_repository);
    }

    [Fact]
    public async Task ReturnOkWithFilesWhenRepositoryContainsFiles()
    {
        var expectedFiles = new List<SyncFile>
        {
            new("file1.txt", "/local/file1.txt", "/remote/file1.txt"),
            new("file2.txt", "/local/file2.txt", "/remote/file2.txt"),
        };
        Result<IReadOnlyList<SyncFile>, string> repositoryResult = expectedFiles;
        _ = _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(repositoryResult));

        Result<IReadOnlyList<SyncFile>, string> result = await _sut.GetSyncFilesAsync(TestContext.Current.CancellationToken);

        Result<IReadOnlyList<SyncFile>, string>.Ok ok = result.ShouldBeOfType<Result<IReadOnlyList<SyncFile>, string>.Ok>();
        ok.Value.ShouldBe(expectedFiles);
    }

    [Fact]
    public async Task ReturnOkWithEmptyListWhenRepositoryContainsNoFiles()
    {
        Result<IReadOnlyList<SyncFile>, string> repositoryResult = new List<SyncFile>();
        _ = _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(repositoryResult));

        Result<IReadOnlyList<SyncFile>, string> result = await _sut.GetSyncFilesAsync(TestContext.Current.CancellationToken);

        Result<IReadOnlyList<SyncFile>, string>.Ok ok = result.ShouldBeOfType<Result<IReadOnlyList<SyncFile>, string>.Ok>();
        ok.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReturnErrorWhenRepositoryReturnsError()
    {
        Result<IReadOnlyList<SyncFile>, string> repositoryResult = "retrieval failed";
        _ = _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(repositoryResult));

        Result<IReadOnlyList<SyncFile>, string> result = await _sut.GetSyncFilesAsync(TestContext.Current.CancellationToken);

        Result<IReadOnlyList<SyncFile>, string>.Error error = result.ShouldBeOfType<Result<IReadOnlyList<SyncFile>, string>.Error>();
        error.Reason.ShouldBe("retrieval failed");
    }
}
