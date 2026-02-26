using AStar.Dev.Utilities;

namespace AStar.Dev.Utilities.Tests.Unit;

public sealed class PathOperationExtensionsShould
{
    [Fact]
    public void CombinePath_ReturnsCombinedPathForRelativeSegments()
    {
        var basePath = Path.Combine("root", "base");

        var result = basePath.CombinePath("child", "file.txt");

        result.ShouldBe(Path.Combine(basePath, "child", "file.txt"));
    }

    [Fact]
    public void CombinePath_AllowsChainingWithoutRootedSegments()
    {
        var result = "base".CombinePath("child").CombinePath("file.txt");

        result.ShouldBe(Path.Combine("base", "child", "file.txt"));
    }

    [Fact]
    public void CombinePath_ThrowsWhenSegmentIsRooted()
    {
        var root = Path.GetPathRoot(Environment.CurrentDirectory) ?? Path.DirectorySeparatorChar.ToString();
        var rootedSegment = Path.Combine(root, "rooted");

        _ = Should.Throw<ArgumentException>(() => "base".CombinePath(rootedSegment));
    }

    [Fact]
    public void CombinePath_ThrowsWhenSegmentsArrayIsNull() => Should.Throw<ArgumentNullException>(() => "base".CombinePath(null!));

    [Fact]
    public void CombinePath_ThrowsWhenAnySegmentIsNull() => Should.Throw<ArgumentNullException>(() => "base".CombinePath(new[] { "child", (string)null! }));
}
