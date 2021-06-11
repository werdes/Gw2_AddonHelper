using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;

namespace Gw2_AddonHelper.AddonLib.Extensions
{
    public static class ILoggerExtensions
    {
        public static void LogObject(this ILogger logger, object obj, LogLevel logLevel = LogLevel.Debug, Formatting formatting = Formatting.Indented)
        {
            string json = JsonConvert.SerializeObject(obj, formatting);
            string[] lines = json.Split('\n').Select(x => x.Replace("\r", "")).ToArray();

            foreach (string line in lines)
            {
                logger.Log(logLevel, line);
            }
        }
    }
}
