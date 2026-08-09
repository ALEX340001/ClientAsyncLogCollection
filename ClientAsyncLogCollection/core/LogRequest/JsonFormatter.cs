using ClientAsyncLogCollection.utils;
using ClientAsyncLogCollection.Utils;
using ClientAsyncLogCollection.Utils.Security.SaveConf;
using MyCompany.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static ClientAsyncLogCollection.core.LogRequest.LogRequest;
namespace ClientAsyncLogCollection.core.LogRequest

{
    internal class JsonFormatter
    {
        private static readonly FileLogger logger = LoggerFactory.Logger;
        public static async Task GetFormat(List<LogEventData> finalLogs, string folderPath, string logNameFile, string levelsPart)
        {

            // Объект создается сразу со значениями: "Logs", Info, 10MB, 30 дней, Mask=true

            logger.Info($"=== JsonFormater.GetFormat ===");
            logger.Info($"finalLogs: {finalLogs}");

            // 1. Убедимся, что корневая папка существует
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                logger.Info($"Создана корневая папка: {folderPath}");
            }

            // 2. Очищаем имя приложения для использования в имени подпапки
            string sanitizedAppName = SanitizeFolderName(logNameFile);
            string appFolderPath = Path.Combine(folderPath, sanitizedAppName);

            // 3. Создаём подпапку приложения, если её нет
            if (!Directory.Exists(appFolderPath))
            {
                Directory.CreateDirectory(appFolderPath);
                logger.Info($"Создана папка для приложения: {appFolderPath}");
            }

            // 4. Формируем имя файла
            string fileName = $"log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{logNameFile}_{levelsPart}.json";


            // Очищаем имя файла от недопустимых символов
            string safeFileName = string.Concat(fileName.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
            string fullPath = Path.Combine(appFolderPath, safeFileName);

            // 5. Сериализация и запись
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string jsonOutput = JsonSerializer.Serialize(finalLogs, options);
            await File.WriteAllTextAsync(fullPath, jsonOutput, Encoding.UTF8);

            logger.Info($"Файл сохранён: {fullPath}");
            logger.Info($"=== JsonFormater.GetFormat ===");
        }

        private static string SanitizeFolderName(string name)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
                name = name.Replace(c, '_');
            return name.Trim();
        }
    }
}
