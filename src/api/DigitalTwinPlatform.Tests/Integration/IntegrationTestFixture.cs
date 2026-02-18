using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DigitalTwinPlatform.Infrastructure.Persistence;
using Xunit;

namespace DigitalTwinPlatform.Tests.Integration;

public class IntegrationTestFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IServiceScope _scope;
    private bool _disposed = false;

    public IntegrationTestFixture()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<DigitalTwinDbContext>));
                    
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database for testing
                    services.AddDbContext<DigitalTwinDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("DigitalTwinTestDb");
                    });

                    // Remove any hosted services that might interfere with tests
                    var hostedServices = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();
                    foreach (var hostedService in hostedServices)
                    {
                        services.Remove(hostedService);
                    }
                });
            });

        _scope = _factory.Services.CreateScope();
        
        // Ensure database is created
        var dbContext = _scope.ServiceProvider.GetRequiredService<DigitalTwinDbContext>();
        dbContext.Database.EnsureCreated();
    }

    public HttpClient CreateClient()
    {
        var client = _factory.CreateClient();
        
        // Add default headers for authenticated requests
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
        client.DefaultRequestHeaders.Add("X-Tenant-ID", "test-tenant");
        
        return client;
    }

    public DigitalTwinDbContext GetDbContext()
    {
        return _scope.ServiceProvider.GetRequiredService<DigitalTwinDbContext>();
    }

    public T GetService<T>() where T : notnull
    {
        return _scope.ServiceProvider.GetRequiredService<T>();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _scope?.Dispose();
            _factory?.Dispose();
            _disposed = true;
        }
    }
}

[CollectionDefinition("Sequential")]
public class SequentialCollection : ICollectionFixture<IntegrationTestFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}