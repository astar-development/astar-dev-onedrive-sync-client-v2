using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;
using Shouldly;

namespace AStar.Dev.OneDrive.Sync.Client.Domain.Tests.Entities;

public sealed class SyncFileTests
{
    [Fact]
    public void Constructor_WithValidArguments_CreatesInstance()
    {
        var syncFile = new SyncFile("document.pdf", "/local/document.pdf", "/remote/document.pdf");

        syncFile.Name.ShouldBe("document.pdf");
        syncFile.LocalPath.ShouldBe("/local/document.pdf");
        syncFile.RemotePath.ShouldBe("/remote/document.pdf");
    }

    [Theory]
    [InlineData("", "/local/path", "/remote/path")]
    [InlineData("  ", "/local/path", "/remote/path")]
    [InlineData("name.txt", "", "/remote/path")]
    [InlineData("name.txt", "  ", "/remote/path")]
    [InlineData("name.txt", "/local/path", "")]
    [InlineData("name.txt", "/local/path", "  ")]
    public void Constructor_WithNullOrWhitespaceArguments_ThrowsArgumentException(
        string name, string localPath, string remotePath) => Should.Throw<ArgumentException>(
            () => new SyncFile(name, localPath, remotePath));

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentNullException() => Should.Throw<ArgumentNullException>(
            () => new SyncFile(null!, "/local/path", "/remote/path"));
}
