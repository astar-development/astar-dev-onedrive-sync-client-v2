using AStar.Dev.OneDrive.Sync.Client.Domain.Entities;

namespace AStar.Dev.OneDrive.Sync.Client.Domain.Tests.Entities;

public sealed class SyncFileShould
{
    [Fact]
    public void CreateValidInstanceWhenParametersAreValid()
    {
        var syncFile = new SyncFile("document.pdf", "/local/document.pdf", "/remote/document.pdf");

        syncFile.Name.ShouldBe("document.pdf");
        syncFile.LocalPath.ShouldBe("/local/document.pdf");
        syncFile.RemotePath.ShouldBe("/remote/document.pdf");
    }
}
