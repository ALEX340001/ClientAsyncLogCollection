using MyCompany.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientAsyncLogCollection.Utils;
namespace ClientAsyncLogCollection.core.SearchWordPlanerTasks
{
    public class ProviderScanner : IProviderScanner
    {
        

        private static readonly FileLogger logger = LoggerFactory.Logger;

        public async Task<Dictionary<string, (string RealName, string LogName)>> ScanAsync(int scanHours)
        {
            var result = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);
            var logsToScan = new[] { "Application", "System" };
            var queryTemplate = $"*[System[TimeCreated[timediff(@SystemTime) <= {scanHours * 3600000L}]]]";

            await Task.Run(() =>
            {
                foreach (var logName in logsToScan)
                {
                    var query = new EventLogQuery(logName, PathType.LogName, queryTemplate);
                    using var reader = new EventLogReader(query);
                    EventRecord record;
                    while ((record = reader.ReadEvent()) != null)
                    {
                        string provider = record.ProviderName;
                        if (!string.IsNullOrEmpty(provider) && !result.ContainsKey(provider))
                        {
                            result[provider] = (provider, logName);
                        }
                    }
                }
            });
            logger?.Info($"Найдено {result.Count} уникальных провайдеров.");
            return result;
        }
    }
}
