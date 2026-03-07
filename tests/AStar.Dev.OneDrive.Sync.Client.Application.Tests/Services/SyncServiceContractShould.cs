using System.Reflection;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Tests.Services;

public sealed class SyncServiceContractShould
{
    [Fact]
    public void DefinePauseResumeAndCancelOperationsWhenPhase2ContractsAreIntroduced()
    {
        MethodInfo[] methods = typeof(ISyncService).GetMethods();

        methods.Any(x => x.Name == "PauseSyncAsync").ShouldBeTrue();
        methods.Any(x => x.Name == "ResumeSyncAsync").ShouldBeTrue();
        methods.Any(x => x.Name == "CancelSyncAsync").ShouldBeTrue();
    }
}
