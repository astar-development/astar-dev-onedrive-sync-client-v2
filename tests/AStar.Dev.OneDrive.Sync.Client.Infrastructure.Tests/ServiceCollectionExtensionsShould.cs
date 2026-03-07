using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Authentication;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Tests;

public sealed class ServiceCollectionExtensionsShould
{
    [Fact]
    public void RegisterIMigrationServiceAsSingleton()
    {
        IServiceCollection services = new ServiceCollection().AddInfrastructure();
        ServiceDescriptor? descriptor = services.SingleOrDefault(sd => sd.ServiceType == typeof(IMigrationService));
        _ = descriptor.ShouldNotBeNull();
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Singleton);
        descriptor.ImplementationType.ShouldBe(typeof(SqliteDatabaseMigrator));
    }

    [Fact]
    public void ResolveIMigrationServiceAsSqliteDatabaseMigrator()
    {
        using ServiceProvider provider = new ServiceCollection().AddInfrastructure().BuildServiceProvider();
        IMigrationService migrationService = provider.GetRequiredService<IMigrationService>();
        _ = migrationService.ShouldBeOfType<SqliteDatabaseMigrator>();
    }

    [Fact]
    public void ResolveIAccountSessionServiceAsOneDriveAccountSessionService()
    {
        using ServiceProvider provider = new ServiceCollection().AddInfrastructure().BuildServiceProvider();
        IAccountSessionService accountSessionService = provider.GetRequiredService<IAccountSessionService>();
        _ = accountSessionService.ShouldBeOfType<OneDriveAccountSessionService>();
    }

    [Fact]
    public void RegisterIOneDriveGraphClient()
    {
        IServiceCollection services = new ServiceCollection().AddInfrastructure();
        ServiceDescriptor? descriptor = services.SingleOrDefault(sd => sd.ServiceType == typeof(IOneDriveGraphClient));
        _ = descriptor.ShouldNotBeNull();
        _ = descriptor.ImplementationFactory.ShouldNotBeNull();
    }

    [Fact]
    public void ResolveIOneDriveGraphClientAsOneDriveGraphClient()
    {
        using ServiceProvider provider = new ServiceCollection().AddInfrastructure().BuildServiceProvider();
        IOneDriveGraphClient graphClient = provider.GetRequiredService<IOneDriveGraphClient>();
        _ = graphClient.ShouldBeOfType<OneDriveGraphClient>();
    }

    [Fact]
    public void RegisterIHttpClientFactory()
    {
        using ServiceProvider provider = new ServiceCollection().AddInfrastructure().BuildServiceProvider();
        IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();
        _ = factory.ShouldNotBeNull();
    }
}
