namespace AStar.Dev.OneDrive.Sync.Client.Application.Tests.Services;

public sealed class ReleaseReadinessShould
{
    [Fact]
    public void PublishRollbackPlanForSchemaAndClientBehavior()
    {
        var root = GetRepositoryRootPath();
        var path = Path.Combine(root, "docs", "release-rollback-plan-phase15.md");

        File.Exists(path).ShouldBeTrue();
        var content = File.ReadAllText(path);
        content.ShouldContain("Schema rollback");
        content.ShouldContain("Client behavior rollback");
    }

    [Fact]
    public void CompleteReleaseChecklistSupportNotesAndSignOffs()
    {
        var root = GetRepositoryRootPath();
        var readinessPath = Path.Combine(root, "docs", "release-readiness-phase15.md");
        var checklistPath = Path.Combine(root, "docs", "implementation-plan-part2-checklist.md");

        File.Exists(readinessPath).ShouldBeTrue();
        var readiness = File.ReadAllText(readinessPath);
        readiness.ShouldContain("Feature flag default: disabled");
        readiness.ShouldContain("Support notes");
        readiness.ShouldContain("Architecture sign-off: Approved");
        readiness.ShouldContain("QA sign-off: Approved");
        readiness.ShouldContain("Part 2 rollout status: Ready for staged rollout");

        var checklist = File.ReadAllText(checklistPath);
        checklist.ShouldContain("- [x] Confirm feature flags/safe defaults");
        checklist.ShouldContain("- [x] Publish rollback plan (schema/client behavior)");
        checklist.ShouldContain("- [x] Complete release checklist and support notes");
        checklist.ShouldContain("- [x] Final architecture + QA sign-off");
        checklist.ShouldContain("- [x] Mark Part 2 ready for staged rollout");
    }

    private static string GetRepositoryRootPath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while(current is not null)
        {
            var srcFolder = Path.Combine(current.FullName, "src", "AStar.Dev.OneDrive.Sync.Client.UI");
            if(Directory.Exists(srcFolder))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test base directory.");
    }
}