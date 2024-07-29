using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace Tanzy.Readwise.Intergration.Tests.Fixtures;

public class MockoonFixture
{
    private readonly IContainer _mockoon;

    public MockoonFixture()
    {
        _mockoon = new ContainerBuilder()
            .WithImage("mockoon/cli:6.2.0")
            .WithPortBinding(3000, true)
            .WithEntrypoint("mockoon-cli", "start")
            .WithCommand(
                "--data",
                "/data/Readwise.json",
                "--port",
                "3000",
                "--log-transaction")
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilPortIsAvailable(3000))
            .WithName($"Mockoon-{Guid.NewGuid():N}")
            .WithBindMount(
                $"{Directory.GetCurrentDirectory().Replace("\\", "/")}/mounts/mockoon",
                "/data",
                AccessMode.ReadOnly)
            .Build();
    }

    public int Port => _mockoon.GetMappedPublicPort(3000);

    public string MockoonHostname => _mockoon.Hostname;

    public async Task StopAsync()
    {
        await _mockoon.StopAsync();
    }

    public async Task StartAsync()
    {
        await _mockoon.StartAsync();
    }
}
