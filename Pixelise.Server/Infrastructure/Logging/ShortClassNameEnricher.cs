using Serilog.Core;
using Serilog.Events;

namespace Pixelise.Server.Infrastructure.Logging;

public class ShortClassNameEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!logEvent.Properties.TryGetValue("SourceContext", out var sourceContextValue))
        {
            return;
        }

        var fullName = sourceContextValue.ToString().Trim('"');

        var lastDot = fullName.LastIndexOf('.');
        var shortName = lastDot >= 0 ? fullName[(lastDot + 1)..] : fullName;

        var property = propertyFactory.CreateProperty("ClassName", shortName);
        logEvent.AddOrUpdateProperty(property);
    }
}