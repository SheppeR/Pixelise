using Pixelise.Server.Infrastructure.Kernel;

namespace Pixelise.Server.Infrastructure.Plugins;

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