using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Pixelise.Server.Infrastructure.Logging;

public static class LoggerExtensions
{
    private static void Log(
        ILogger logger,
        LogLevel level,
        string message,
        Exception? exception,
        string memberName,
        string sourceFilePath,
        int sourceLineNumber)
    {
        var className = Path.GetFileNameWithoutExtension(sourceFilePath);

        if (memberName == ".ctor")
        {
            memberName = "ctor";
        }

        // ===== COULEURS ANSI (console) =====
        const string Cyan = "\u001b[36m";
        const string Yellow = "\u001b[33m";
        const string Gray = "\u001b[90m";
        const string Reset = "\u001b[0m";

        var coloredClass = $"{Cyan}{className}{Reset}";
        var coloredMethod = $"{Yellow}{memberName}{Reset}";
        var coloredLine = $"{Gray}{sourceLineNumber}{Reset}";

        var finalMessageColored = $"{coloredClass}.{coloredMethod}({coloredLine}) → {message}";
        var finalMessagePlain = $"{className}.{memberName}({sourceLineNumber}) → {message}";

        // 👉 On pousse la version CLEAN dans le scope
        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["PlainMessage"] = finalMessagePlain
               }))
        {
            if (exception != null)
            {
                logger.Log(level, exception, finalMessageColored);
            }
            else
            {
                logger.Log(level, finalMessageColored);
            }
        }
    }

    public static void Section(this ILogger logger, string sectionName)
    {
        var sb = new StringBuilder();
        sb.Append($"-[ {sectionName.ToUpper()} ]");

        while (sb.Length < 79)
        {
            sb.Insert(0, "=");
        }

        logger.LogInformation(sb.ToString());
    }

    extension(ILogger logger)
    {
        public void Info(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log(logger, LogLevel.Information, message, null, memberName, sourceFilePath, sourceLineNumber);
        }

        public void Debug(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log(logger, LogLevel.Debug, message, null, memberName, sourceFilePath, sourceLineNumber);
        }

        public void Warn(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log(logger, LogLevel.Warning, message, null, memberName, sourceFilePath, sourceLineNumber);
        }

        public void Error(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log(logger, LogLevel.Error, message, null, memberName, sourceFilePath, sourceLineNumber);
        }

        public void Error(Exception exception,
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log(logger, LogLevel.Error, message, exception, memberName, sourceFilePath, sourceLineNumber);
        }
    }
}