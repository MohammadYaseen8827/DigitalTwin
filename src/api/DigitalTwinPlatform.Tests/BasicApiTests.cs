using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DigitalTwinPlatform.Infrastructure.Persistence;
using DigitalTwinPlatform.Application.Machines.Models;

namespace DigitalTwinPlatform.Tests;

public class HealthTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory;

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/health");
        response.EnsureSuccessStatusCode();
    }
}

public class MachinesTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MachinesTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DigitalTwinDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                services.AddDbContext<DigitalTwinDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });
    }

    [Fact]
    public async Task GetMachines_ReturnsEmpty()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/machines");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<MachineDto>>();
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
