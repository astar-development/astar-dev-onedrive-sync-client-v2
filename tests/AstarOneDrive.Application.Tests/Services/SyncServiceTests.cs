using AStar.Dev.Functional.Extensions;
using AstarOneDrive.Application.Interfaces;
using AstarOneDrive.Application.Services;
using AstarOneDrive.Domain.Entities;
using AstarOneDrive.Domain.Interfaces;
using NSubstitute;
using Shouldly;

namespace AstarOneDrive.Application.Tests.Services;

public sealed class SyncServiceTests
{
    private readonly ISyncFileRepository _repository;
    private readonly ISyncService _sut;

    public SyncServiceTests()
    {
        _repository = Substitute.For<ISyncFileRepository>();
        _sut = new SyncService(_repository);
    }

    [Fact]
    public async Task GetSyncFilesAsync_WhenRepositoryReturnsFiles_ReturnsOkWithFiles()
    {
        var expectedFiles = new List<SyncFile>
        {
            new("file1.txt", "/local/file1.txt", "/remote/file1.txt"),
            new("file2.txt", "/local/file2.txt", "/remote/file2.txt"),
        };
        Result<IReadOnlyList<SyncFile>, string> repositoryResult = expectedFiles;
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(repositoryResult));

        var result = await _sut.GetSyncFilesAsync(TestContext.Current.CancellationToken);

        var ok = result.ShouldBeOfType<Result<IReadOnlyList<SyncFile>, string>.Ok>();
        ok.Value.ShouldBe(expectedFiles);
    }

    [Fact]
    public async Task GetSyncFilesAsync_WhenRepositoryReturnsEmptyList_ReturnsOkWithEmptyList()
    {
        Result<IReadOnlyList<SyncFile>, string> repositoryResult = new List<SyncFile>();
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(repositoryResult));

        var result = await _sut.GetSyncFilesAsync(TestContext.Current.CancellationToken);

        var ok = result.ShouldBeOfType<Result<IReadOnlyList<SyncFile>, string>.Ok>();
        ok.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetSyncFilesAsync_WhenRepositoryReturnsError_ReturnsError()
    {
        Result<IReadOnlyList<SyncFile>, string> repositoryResult = "retrieval failed";
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(repositoryResult));

        var result = await _sut.GetSyncFilesAsync(TestContext.Current.CancellationToken);

        var error = result.ShouldBeOfType<Result<IReadOnlyList<SyncFile>, string>.Error>();
        error.Reason.ShouldBe("retrieval failed");
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException() => Should.Throw<ArgumentNullException>(
            () => new SyncService(null!));
}
