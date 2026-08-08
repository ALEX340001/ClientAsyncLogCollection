using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyCompany.Logging;
using ClientAsyncLogCollection.Utils;
namespace ClientAsyncLogCollection.utils
{

    internal class GetLevelDisplayNameSafe
    {
        

        private static readonly FileLogger logger = LoggerFactory.Logger;
        protected internal static string Call(EventRecord eventRecord)
        {
            try
            {
                return eventRecord.LevelDisplayName;
            }
            catch (EventLogNotFoundException)
            {
                // Провайдер не найден – используем числовой уровень
                return $"Level {eventRecord.Level}";
            }
            catch (Exception ex)
            {
                logger.Warn($"Не удалось получить LevelDisplayName: {ex.Message}");
                return $"Level {eventRecord.Level}";
            }
        }
    }
}
