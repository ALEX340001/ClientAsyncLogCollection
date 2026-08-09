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
    internal class XmlFormatter
    {
        private static readonly FileLogger logger = LoggerFactory.Logger;
        public static async Task GetFormat(List<LogEventData> finalLogs, string folderPath, string logNameFile, string levelsPart)
        {
            try
            {
                logger.Info($"=== XmlFormater.GetFormat START ===");
                logger.Info($"folderPath: {folderPath}");
                logger.Info($"logNameFile: '{logNameFile}'");
                logger.Info($"levelsPart: '{levelsPart}'");
                logger.Info($"finalLogs count: {finalLogs?.Count ?? 0}");

                logger.Info($"=== XmlFormater.GetFormat ===");
                logger.Info($"finalLogs: {finalLogs}");

                // 1. Убедимся, что корневая папка существует
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    logger.Info($"Создана корневая папка: {folderPath}");
                }


                // 2. Очищаем имя приложения для использования в имени подпапки
                string sanitizedAppName = SanitizeFolderName(logNameFile);
                logger.Info($"sanitizedAppName: {sanitizedAppName}");

                string appFolderPath = Path.Combine(folderPath, sanitizedAppName);
                logger.Info($"appFolderPath: {appFolderPath}");


                // 3. Создаём подпапку приложения, если её нет
                if (!Directory.Exists(appFolderPath))
                {
                    Directory.CreateDirectory(appFolderPath);
                    logger.Info($"Создана папка для приложения: {appFolderPath}");
                }


                string levelSuffix = string.IsNullOrEmpty(levelsPart) ? "" : $"_{levelsPart}";
                logger.Info($"levelSuffix: {levelSuffix}");

                string fileName = $"log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{sanitizedAppName}{levelSuffix}.xml";
                logger.Info($"fileName: {fileName}");

                string safeFileName = string.Concat(fileName.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
                logger.Info($"safeFileName: {safeFileName}");

                string fullPath = Path.Combine(appFolderPath, safeFileName);
                logger.Info($"fullPath: {fullPath}");



                // Собираем все RawXml в единый XML-документ
                var xmlBuilder = new StringBuilder();
                logger.Info($"xmlBuilder: {xmlBuilder}");

                xmlBuilder.AppendLine("<Events>");
                logger.Info($"xmlBuilder.AppendLine: {xmlBuilder.AppendLine("<Events>")}");



                logger.Info($"Вход в цикл : foreach (var log in finalLogs)");
                foreach (var log in finalLogs)
                {
                    xmlBuilder.AppendLine(log.RawXml);

                }
                logger.Info($"результат после цикла: {finalLogs}");
                logger.Info($"finalLogs: {finalLogs}");


                xmlBuilder.AppendLine("</Events>");

                try
                {
                    await File.WriteAllTextAsync(fullPath, xmlBuilder.ToString(), Encoding.UTF8);
                    logger.Info($"Файл успешно сохранён: {fullPath}");
                }
                catch (Exception ex)
                {
                    logger.Error($"Ошибка записи файла {fullPath}: {ex.Message}");
                }
            }

            catch (Exception ex)
            {
                logger.Error($"Ошибка в XmlFormater.GetFormat: {ex.Message}");
                logger.Error($"Stack trace: {ex.StackTrace}");
                throw; // Временно – программа упадёт, и вы увидите ошибку в консоли
            }
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
