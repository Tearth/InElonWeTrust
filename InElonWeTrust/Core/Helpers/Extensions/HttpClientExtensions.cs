using System;
using System.Net.Http;
using System.Threading.Tasks;
using NLog;

namespace InElonWeTrust.Core.Helpers.Extensions
{
    public static class HttpClientExtensions
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task<string> GetStringWithRetriesAsync(this HttpClient httpClient, string requestUri)
        {
            Exception lastException = null;
            for (var i = 0; i < Constants.MaxHttpAttempts; i++)
            {
                try
                {
                    return await httpClient.GetStringAsync(requestUri);
                }
                catch (Exception e)
                {
                    Logger.Warn($"Failed to retrieve data from the HTTP client during attempt {i + 1} " +
                                 $"({e.GetType().Name}: {e.Message})");
                    lastException = e;

                    await Task.Delay(Constants.DelayMsBetweenHttpAttempts);
                }
            }

            if (lastException != null)
            {
                throw lastException;
            }

            return null;
        }
    }
}
