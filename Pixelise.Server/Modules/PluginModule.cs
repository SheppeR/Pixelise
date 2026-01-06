using System.Reflection;
using Microsoft.Extensions.Logging;
using Pixelise.Server.Infrastructure.Kernel;
using Pixelise.Server.Infrastructure.Logging;

namespace Pixelise.Server.Modules;

public sealed class PluginModule(IServiceProvider services, ILogger<PluginModule> logger) : IServerModule
{
    private readonly List<IServerPlugin> _loaded = [];
    public string Name => "Plugins";

    public Task StartAsync(CancellationToken ct)
    {
        logger.Section(Name);

        logger.Info($"Loading module : {Name}.");

        LoadPlugins();
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        foreach (var plugin in _loaded)
        {
            logger.Info($"Unloading plugin: {plugin.Name}");
            plugin.OnUnload();
        }

        logger.Info($"{Name} module stopped.");

        _loaded.Clear();
        return Task.CompletedTask;
    }

    private void LoadPlugins()
    {
        var pluginsDir = Path.Combine(AppContext.BaseDirectory, "Plugins");
        Directory.CreateDirectory(pluginsDir);

        foreach (var file in Directory.GetFiles(pluginsDir, "*.dll"))
        {
            try
            {
                var asm = Assembly.LoadFrom(file);

                var types = asm.GetTypes()
                    .Where(t => typeof(IServerPlugin).IsAssignableFrom(t) && !t.IsAbstract);

                foreach (var type in types)
                {
                    var plugin = (IServerPlugin)Activator.CreateInstance(type)!;
                    plugin.OnLoad(services);
                    _loaded.Add(plugin);

                    logger.Info($"Loaded plugin: {plugin.Name}");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to load plugin: {Path.GetFileName(file)}");
            }
        }
    }
}