using ClientAsyncLogCollection.utils;
using ClientAsyncLogCollection.Utils.Security.SaveConf;
using MyCompany.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ClientAsyncLogCollection.Utils;

namespace ClientAsyncLogCollection.core.LogRequest
{
    internal class LogRequest
    {
        public class LogEventData
        {
            public int Id { get; set; }
            public DateTime? TimeCreated { get; set; }
            public string ProviderName { get; set; }
            public string LevelDisplayName { get; set; }
            public int Level { get; set; }
            public string Description { get; set; }
            public string RawXml { get; set; } // На случай, если XmlFormater захочет работать с чистым XML
        }
        

        private static readonly FileLogger logger = LoggerFactory.Logger;

        private static int _isProcessing = 0;

        public static async Task GetLogAsync(AppConfig config, Dictionary<string, (string RealName, string LogName)> providerMap)
        {



            logger.Error($"=== LogRequest.GetLogAsync ===");


            if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) != 0)
            {
                logger.Warn("GetLogAsync уже выполняется, повторный вызов пропущен");
                return;
            }
            try
            {

                // Объект создается сразу со значениями: "Logs", Info, 10MB, 30 дней, Mask=true

                logger.Error($"=== LogRequest.GetLogAsync ===");



                string xmlContent = "";



                // 1. Строим словарь: журнал -> список провайдеров, которые в него пишут

                var logProvidersMap = new Dictionary<string, List<string>>();
                var nameResolver = new GetLogNameForProvider(); // твой класс







                foreach (AppConfig app in config.EnabledApplications)
                {
                    string appName = app.Name;
                    string forcedLog = app.Log;

                    // Приоритет 1: явный журнал в конфиге
                    if (!string.IsNullOrEmpty(forcedLog))
                    {
                        string logNameFile = forcedLog;
                        logger.Info($"Провайдер '{appName}' -> журнал '{logNameFile}' (явное указание)");
                        if (!logProvidersMap.ContainsKey(logNameFile))
                            logProvidersMap[logNameFile] = new List<string>();
                        logProvidersMap[logNameFile].Add(appName);
                        continue;
                    }

                    // Приоритет 2: кэш провайдеров (providerMap)
                    if (providerMap != null && providerMap.TryGetValue(appName, out var mapping))
                    {
                        string realName = mapping.RealName;
                        string logNameFile = mapping.LogName;
                        logger.Info($"Провайдер '{appName}' (реальное имя '{realName}') -> журнал '{logNameFile}' (из кэша)");
                        if (!logProvidersMap.ContainsKey(logNameFile))
                            logProvidersMap[logNameFile] = new List<string>();
                        logProvidersMap[logNameFile].Add(realName); // важно: добавляем реальное имя провайдера!
                        continue;
                    }

                    // Приоритет 3: старый метод автоопределения (как fallback)
                    string autoLog = nameResolver.Call(appName);
                    if (!string.IsNullOrEmpty(autoLog))
                    {
                        logger.Info($"Провайдер '{appName}' -> журнал '{autoLog}' (автоопределение)");
                        if (!logProvidersMap.ContainsKey(autoLog))
                            logProvidersMap[autoLog] = new List<string>();
                        logProvidersMap[autoLog].Add(appName);
                    }
                    else
                    {
                        logger.Warn($"Провайдер '{appName}' не сопоставлен ни с одним журналом, пропускаем.");
                    }
                }



                // 2. Для каждой группы формируем свой запрос и читаем события
                var allEvents = new List<LogEventData>();
                foreach (var kvp in logProvidersMap)
                {
                    string logName = kvp.Key;          // например, "System"
                    var providers = kvp.Value;        // список провайдеров в этом журнале

                    // Строим часть про провайдеров для этого журнала
                    string appsQuery = string.Join(" or ", providers.Select(p => $"Provider[@Name='{p}']"));

                    // Уровни и время – общие для всех
                    string levelsQuery = string.Join(" or ", config.Flags.Select(f => $"Level={f}"));
                    string query = $"*[System[({levelsQuery}) and ({appsQuery})]]";



                    var logQuery = new EventLogQuery(logName, PathType.LogName, query);
                    logger.Info($"logQuery: {logQuery}");

                    var txtBuilder = new StringBuilder(); // Сюда будем собирать читаемый текст


                    Task<List<LogEventData>> logTask = Task.Run(() =>
                    {
                        var eventsList = new List<LogEventData>();
                        logger.Info($"eventsList: {eventsList}");

                        using (var reader = new EventLogReader(logQuery))
                        {
                            EventRecord eventRecord;
                            while ((eventRecord = reader.ReadEvent()) != null)
                            {
                                // Фильтр по времени (если DaysToCollect > 0)
                                if (config.DaysToCollect > 0 && eventRecord.TimeCreated.HasValue)
                                {
                                    var ageDays = (DateTime.Now - eventRecord.TimeCreated.Value).TotalDays;
                                    if (ageDays > config.DaysToCollect)
                                        continue;   // событие старше заданного периода, пропускаем
                                }

                                // Создаем чистый объект данных
                                var logEntry = new LogEventData
                                {
                                    Id = eventRecord.Id,
                                    TimeCreated = eventRecord.TimeCreated,
                                    ProviderName = eventRecord.ProviderName,
                                    LevelDisplayName = GetLevelDisplayNameSafe.Call(eventRecord),
                                    Level = (int)eventRecord.Level,
                                    Description = eventRecord.FormatDescription() ?? "[Нет описания]",
                                    RawXml = eventRecord.ToXml() // Сохраняем сырой XML
                                };
                                logger.Info($"logEntry: {logEntry}");


                                eventsList.Add(logEntry);


                            }
                        }
                        return eventsList; // Возвращаем чистый массив данных
                    });



                    // Ожидаем массив объектов
                    List<LogEventData> finalLogs = await logTask;

                    logger.Info($"config.Format = '{config.Format}'");
                    logger.Info($"Всего событий: {finalLogs.Count}");
                    if (finalLogs.Any())
                    {
                        var first = finalLogs.First();
                        logger.Info($"Первое событие: Level={first.Level}, LevelDisplayName={first.LevelDisplayName}");
                    }





                    logger.Info($"finalLogs: {finalLogs}");

                    // Словарь для преобразования числового уровня в название (можно вынести в статическое поле класса)
                    var levelNames = new Dictionary<int, string>
                    {
                        { 1, "Critical" },
                        { 2, "Error" },
                        { 3, "Warning" },
                        { 4, "Information" },
                        { 5, "Verbose" }
                    };

                    // Группируем сначала по провайдеру, затем по уровню
                    var grouped = finalLogs
                         .GroupBy(log => log.ProviderName)
                         .SelectMany(providerGroup => providerGroup
                         .GroupBy(log => log.Level)
                         .Select(levelGroup => new
                         {
                             Provider = providerGroup.Key,
                             LevelValue = levelGroup.Key,
                             LevelName = levelNames.GetValueOrDefault(levelGroup.Key, levelGroup.Key.ToString()),
                             Logs = levelGroup.ToList()
                         }));


                    // Определяем итоговую папку для сохранения
                    string outputFolder = !string.IsNullOrEmpty(config.UserFolderPath)
                        ? config.UserFolderPath
                        : config.DefaultFolderPath;

                    if (config.Flags == null || config.Flags.Length == 0)
                    {
                        config.Flags = new[] { 1, 2, 3, 4, 5 };
                        logger.Info("Flags не заданы, используем все уровни событий");
                    }


                    // Для каждой группы вызываем форматтер
                    foreach (var item in grouped)
                    {
                        string suffix = $"_{item.LevelName}";   // например, "_Error"

                        logger.Info($"Формат из конфигурации: '{config.Format}' (после ToLower: '{(config.Format?.ToLower())}')");
                        logger.Info($"Папка для сохранения: {config.DefaultFolderPath}");
                        logger.Info($"Количество событий для обработки: {finalLogs.Count}");

                        switch (config.Format?.ToLower())
                        {
                            case "xml":
                                await XmlFormatter.GetFormat(item.Logs, outputFolder, item.Provider, suffix);
                                break;
                            case "json":
                                await JsonFormatter.GetFormat(item.Logs, outputFolder, item.Provider, suffix);
                                break;
                        }
                    }

                    logger.Info($"=== LogRequest.GetLogAsync ===");
                }
            }
            finally
            {
                Interlocked.Exchange(ref _isProcessing, 0);
            }

        }
    }
}