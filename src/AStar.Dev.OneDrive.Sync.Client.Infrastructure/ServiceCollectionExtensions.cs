using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Authentication;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Repositories;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;
using Microsoft.Extensions.DependencyInjection;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure;

/// <summary>
/// Provides extension methods for registering Infrastructure services with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Infrastructure layer services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        => services
            .AddSingleton<IMigrationService, SqliteDatabaseMigrator>()
            .AddSingleton<IOneDriveAuthenticationAdapter, OneDriveAuthenticationAdapter>()
            .AddSingleton<ISecureAccountTokenStore, FileBackedSecureAccountTokenStore>()
            .AddSingleton<OneDriveGraphClientOptions>()
            .AddSingleton<IGraphDelayStrategy, GraphDelayStrategy>()
            .AddSingleton<IOneDriveGraphTelemetry, NoOpOneDriveGraphTelemetry>()
            .AddSingleton<IOneDriveGraphClient>(provider =>
            {
                OneDriveGraphClientOptions options = provider.GetRequiredService<OneDriveGraphClientOptions>();
                IGraphDelayStrategy delayStrategy = provider.GetRequiredService<IGraphDelayStrategy>();
                IOneDriveGraphTelemetry telemetry = provider.GetRequiredService<IOneDriveGraphTelemetry>();
                HttpClient httpClient = new() { BaseAddress = options.BaseUri };
                return new OneDriveGraphClient(httpClient, options, delayStrategy, telemetry);
            })
            .AddSingleton<SqliteAccountSessionMetadataRepository>()
            .AddSingleton<IAccountSessionService, OneDriveAccountSessionService>();
}
