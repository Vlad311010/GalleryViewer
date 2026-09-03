using Microsoft.Extensions.Logging;
using Serilog.Context;
using Shared.Enums;

namespace Shared.Extensions
{
    public static class LoggerExtensions
    {
        public const string AreaPropertyName = "Area";

        public static void Info(
            this ILogger logger,
            string message,
            ApplicationArea area,
            params object[] args)
        {
            using (LogContext.PushProperty(AreaPropertyName, area))
                logger.LogInformation(message, args);
        }

        public static void Warning(
            this ILogger logger,
            string message,
            ApplicationArea area,
            params object[] args)
        {
            using (LogContext.PushProperty(AreaPropertyName, area))
                logger.LogWarning(message, args);
        }

        public static void Error(
            this ILogger logger,
            string message,
            ApplicationArea area,
            params object[] args)
        {
            using (LogContext.PushProperty(AreaPropertyName, area))
                logger.LogError(message, args);
        }

        public static void Debug(
            this ILogger logger,
            string message,
            ApplicationArea area,
            params object[] args)
        {
            using (LogContext.PushProperty(AreaPropertyName, area))
                logger.LogDebug(message, args);
        }
    }
}
