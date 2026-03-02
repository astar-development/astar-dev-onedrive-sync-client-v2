using AStar.Dev.OneDrive.Sync.Client.Application.Interfaces;
using AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

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
}
