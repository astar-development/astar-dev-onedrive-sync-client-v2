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
    public async Task GetSyncFilesAsync_WhenRepositoryReturnsFiles_ReturnsFiles()
    {
        var expectedFiles = new List<SyncFile>
        {
            new("file1.txt", "/local/file1.txt", "/remote/file1.txt"),
            new("file2.txt", "/local/file2.txt", "/remote/file2.txt"),
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SyncFile>>(expectedFiles));

        var result = await _sut.GetSyncFilesAsync(TestContext.Current.CancellationToken);

        result.ShouldBe(expectedFiles);
    }

    [Fact]
    public async Task GetSyncFilesAsync_WhenRepositoryReturnsEmptyList_ReturnsEmptyList()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SyncFile>>([]));

        var result = await _sut.GetSyncFilesAsync(TestContext.Current.CancellationToken);

        result.ShouldBeEmpty();
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(
            () => new SyncService(null!));
    }
}
