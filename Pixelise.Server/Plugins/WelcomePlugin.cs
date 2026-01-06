using Pixelise.Server.Utils.Server;

namespace Pixelise.Server.Plugins;

public class WelcomePlugin : IServerPlugin
{
    public string Name => "WelcomePlugin";

    public void OnLoad(IServiceProvider services)
    {
        Console.WriteLine("👋 WelcomePlugin loaded!");
    }

    public void OnUnload()
    {
        Console.WriteLine("❌ WelcomePlugin unloaded.");
    }
}