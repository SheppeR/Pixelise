namespace Pixelise.Server.Infrastructure.Kernel;

public interface IServerModule
{
    string Name { get; }

    Task StartAsync(CancellationToken ct);
    Task StopAsync();
}