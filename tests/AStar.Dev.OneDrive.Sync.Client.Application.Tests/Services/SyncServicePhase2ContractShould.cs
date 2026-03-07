using System.Reflection;
using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;

namespace AStar.Dev.OneDrive.Sync.Client.Application.Tests.Services;

public sealed class SyncServicePhase2ContractShould
{
    [Fact]
    public void DefineQueueConflictAndCheckpointModelsWhenPhase2ContractsAreIntroduced()
    {
        Assembly applicationAssembly = typeof(ISyncService).Assembly;

        applicationAssembly.GetType("AStar.Dev.OneDrive.Sync.Client.Application.Models.SyncQueueItem").ShouldNotBeNull();
        applicationAssembly.GetType("AStar.Dev.OneDrive.Sync.Client.Application.Models.SyncConflict").ShouldNotBeNull();
        applicationAssembly.GetType("AStar.Dev.OneDrive.Sync.Client.Application.Models.SyncCheckpoint").ShouldNotBeNull();
    }

    [Fact]
    public void DefineOrchestrationOperationsWhenPhase2ContractsAreIntroduced()
    {
        MethodInfo[] methods = typeof(ISyncService).GetMethods();

        methods.Any(x => x.Name == "RunDeltaSyncAsync").ShouldBeTrue();
        methods.Any(x => x.Name == "EnqueueUploadAsync").ShouldBeTrue();
        methods.Any(x => x.Name == "EnqueueDownloadAsync").ShouldBeTrue();
    }

    [Fact]
    public void DefineFailureHandlingOperationsWhenPhase2ContractsAreIntroduced()
    {
        MethodInfo[] methods = typeof(ISyncService).GetMethods();

        methods.Any(x => x.Name == "GetFailedOperationsAsync").ShouldBeTrue();
        methods.Any(x => x.Name == "RetryFailedOperationsAsync").ShouldBeTrue();
    }
}
