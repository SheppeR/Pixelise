namespace Pixelise.Server.Infrastructure.Kernel;

public interface IServerPlugin
{
    string Name { get; }

    void OnLoad(IServiceProvider services);
    void OnUnload();
}