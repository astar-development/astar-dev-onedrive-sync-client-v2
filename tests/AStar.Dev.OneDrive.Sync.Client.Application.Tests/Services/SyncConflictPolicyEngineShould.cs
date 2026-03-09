using AStar.Dev.OneDrive.Sync.Client.Application.Models;
using AStar.Dev.OneDrive.Sync.Client.Application.Services;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Tests.Services;

public sealed class SyncConflictPolicyEngineShould
{
    [Fact]
    public void ClassifyEtagMismatchWhenEtagsDiffer()
    {
        var context = new SyncConflictContext("local-1", "remote-2", null, null, false, false, false, false);
        var sut = new SyncConflictPolicyEngine();

        sut.Classify(context).ShouldBe(SyncConflictKind.EtagMismatch);
    }

    [Fact]
    public void ClassifyRenameDeleteWhenRenameAndDeleteArePresent()
    {
        var context = new SyncConflictContext(null, null, null, null, true, false, false, true);
        var sut = new SyncConflictPolicyEngine();

        sut.Classify(context).ShouldBe(SyncConflictKind.RenameDelete);
    }

    [Fact]
    public void ResolveManualPolicyToManualQueueOutcome()
    {
        var context = new SyncConflictContext("same", "same", DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1), false, false, false, false);
        var sut = new SyncConflictPolicyEngine();

        ConflictPolicyDecision decision = sut.Resolve(context, SyncConflictResolutionPolicy.Manual);

        decision.Outcome.ShouldBe(SyncConflictResolutionOutcome.QueueForManualResolution);
    }

    [Fact]
    public void ResolveRenameCopyPolicyToRenameOutcome()
    {
        var context = new SyncConflictContext("same", "same", DateTime.UtcNow, DateTime.UtcNow.AddMinutes(2), false, false, false, false);
        var sut = new SyncConflictPolicyEngine();

        ConflictPolicyDecision decision = sut.Resolve(context, SyncConflictResolutionPolicy.RenameCopy);

        decision.Outcome.ShouldBe(SyncConflictResolutionOutcome.RenameAndProceed);
    }
}
