namespace Pixelise.Server.Utils.Server;

public interface IServerModule
{
    string Name { get; }

    Task StartAsync(CancellationToken ct);
    Task StopAsync();
}