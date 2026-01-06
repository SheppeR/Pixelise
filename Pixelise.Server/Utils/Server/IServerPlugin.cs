namespace Pixelise.Server.Utils.Server;

public interface IServerPlugin
{
    string Name { get; }

    void OnLoad(IServiceProvider services);
    void OnUnload();
}