using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Pixelise.Server.Infrastructure.Logging;

public static class LogBootstrapper
{
    private static bool _latestIsSymlink;
    public static string CurrentLogFilePath { get; private set; } = null!;

    public static void Configure()
    {
        var logsDir = Path.Combine(AppContext.BaseDirectory, "Logs");
        Directory.CreateDirectory(logsDir);

        CleanupOldLogs(logsDir, 10);

        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var logFilePath = Path.Combine(logsDir, $"log-{timestamp}.txt");
        var latestLogPath = Path.Combine(logsDir, "log-latest.txt");

        CurrentLogFilePath = logFilePath;

        // Supprime ancien latest
        if (File.Exists(latestLogPath))
        {
            try
            {
                File.Delete(latestLogPath);
            }
            catch
            {
            }
        }

        // Tentative symlink (Linux OK, Windows si autorisé)
        try
        {
            File.CreateSymbolicLink(latestLogPath, logFilePath);
            _latestIsSymlink = true;
        }
        catch
        {
            _latestIsSymlink = false;
        }

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .WriteTo.Console(
                theme: AnsiConsoleTheme.Code,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .WriteTo.File(
                logFilePath,
                retainedFileCountLimit: 10,
                shared: true,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {PlainMessage}{NewLine}{Exception}"
            )
            .CreateLogger();

        // Fallback Windows sans symlink → copie à la fermeture
        if (!_latestIsSymlink)
        {
            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                try
                {
                    File.Copy(logFilePath, latestLogPath, true);
                }
                catch
                {
                }
            };
        }
    }

    private static void CleanupOldLogs(string logsDir, int maxFiles)
    {
        try
        {
            var files = new DirectoryInfo(logsDir)
                .GetFiles("log-*.txt")
                .OrderByDescending(f => f.CreationTimeUtc)
                .ToList();

            if (files.Count <= maxFiles)
            {
                return;
            }

            foreach (var file in files.Skip(maxFiles))
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                }
            }
        }
        catch
        {
            // ignore
        }
    }
}