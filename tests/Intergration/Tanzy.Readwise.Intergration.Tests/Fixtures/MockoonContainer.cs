using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace Tanzy.Readwise.Intergration.Tests.Fixtures;

public class MockoonContainer
{
    private readonly IContainer _mockoon;
    private bool _mockoonStarted;
    private static readonly Lazy<MockoonContainer> Lazy = new(() => new MockoonContainer());
    private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);

    private MockoonContainer()
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

    public static MockoonContainer Instance => Lazy.Value;

    public ushort ReadwisePort => _mockoon.GetMappedPublicPort(3000);

    public string MockoonHostname => _mockoon.Hostname;

    public async ValueTask DisposeAsync()
    {
        await _mockoon.DisposeAsync();
    }

    public void Dispose()
    {
        CastAndDispose(_mockoon);

        return;

        static void CastAndDispose(IAsyncDisposable resource)
        {
            if (resource is IDisposable resourceDisposable)
            {
                resourceDisposable.Dispose();
            }
            else
            {
                _ = resource.DisposeAsync().AsTask();
            }
        }
    }

    public async Task StartAsync()
    {
        // create async lock
        await Semaphore.WaitAsync();

        try
        {
            if (_mockoonStarted)
            {
                return;
            }

            await _mockoon.StartAsync();

            _mockoonStarted = true;
        }
        finally
        {
            Semaphore.Release();
        }
    }

}