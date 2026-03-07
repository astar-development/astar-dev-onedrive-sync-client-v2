using System.Net.Http;
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
    {
        _ = services
            .AddSingleton<IMigrationService, SqliteDatabaseMigrator>()
            .AddSingleton<IOneDriveAuthenticationAdapter, OneDriveAuthenticationAdapter>()
            .AddSingleton<ISecureAccountTokenStore, FileBackedSecureAccountTokenStore>()
            .AddSingleton<OneDriveGraphClientOptions>()
            .AddSingleton<IOneDriveGraphTelemetry, NoOpOneDriveGraphTelemetry>()
            .AddSingleton<SqliteAccountSessionMetadataRepository>()
            .AddSingleton<IAccountSessionService, OneDriveAccountSessionService>();

        _ = services
            .AddHttpClient<IOneDriveGraphClient, OneDriveGraphClient>((provider, httpClient) =>
            {
                OneDriveGraphClientOptions options = provider.GetRequiredService<OneDriveGraphClientOptions>();
                httpClient.BaseAddress = options.BaseUri;
                httpClient.Timeout = options.RequestTimeout;
            })
            .ConfigurePrimaryHttpMessageHandler(provider =>
            {
                OneDriveGraphClientOptions options = provider.GetRequiredService<OneDriveGraphClientOptions>();
                return new SocketsHttpHandler { PooledConnectionLifetime = options.PooledConnectionLifetime };
            })
            .AddStandardResilienceHandler();

        return services;
    }
}
