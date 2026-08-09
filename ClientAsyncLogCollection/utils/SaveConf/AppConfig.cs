using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using CommandLine;
using MyCompany.Logging;
using ClientAsyncLogCollection.Utils;

namespace ClientAsyncLogCollection.Utils.Security.SaveConf
{
    public class AppConfig
    {

       
            public string Name { get; set; }
            public string Log { get; set; }  // необязательное поле


        // В AppConfig:
        public int ScanHours { get; set; } = 720;           // 7 дней
        public bool ForceProviderRefresh { get; set; } = false;

        // --- Свойства значения по умолчанию ---
        public string DefaultFolderPath { get; set; }
        public string Format { get; set; }
        public string UserFolderPath { get; set; }
        public int DaysToCollect { get; set; } = 1;   // по умолчанию 1 день
        public AppConfig[] EnabledApplications { get; set; } = Array.Empty<AppConfig>();
        public int[] Flags { get; set; } = Array.Empty<int>();

        // --- Логгер  ---
        private static readonly FileLogger logger = LoggerFactory.Logger;

        // --- Путь к стандартному JSON (по умолчанию) ---
        private static readonly string DefaultConfigPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "appsettings.json");

        // --- Командная строка (будет передаваться из Program.Main) ---
        public class CommandLineOptions
        {
            [Option('c', "config", Required = false, HelpText = "Путь к JSON-файлу конфигурации.")]
            public string ConfigFilePath { get; set; }

            [Option('t', Required = false, HelpText = "За сколько дней собрать логи (по умолчанию 1).")]
            public int? DaysToCollect { get; set; }

            [Option('f', "format", Required = false, HelpText = "Формат вывода (json, xml).")]
            public string Format { get; set; }

            [Option("defaultFolder", Required = false, HelpText = "Папка по умолчанию для логов.")]
            public string DefaultFolderPath { get; set; }

            [Option("pl", Required = false, HelpText = "Папка пользователя для логов. - PathLog")]
            public string UserFolderPath { get; set; }

            [Option("apps", Required = false, Separator = ',', HelpText = "Список приложений через запятую.")]
            public IEnumerable<string> EnabledApplications { get; set; }

            [Option("flags", Required = false, Separator = ',', HelpText = "Список числовых флагов через запятую.")]
            public IEnumerable<int> Flags { get; set; }
        }

        // --- Единый метод загрузки (с учётом командной строки) ---
        public static AppConfig Load(CommandLineOptions cmdOptions = null)
        {


            // 1. Определяем путь к JSON-файлу (--config или стандартный)
            string configPath = cmdOptions?.ConfigFilePath;
            if (string.IsNullOrEmpty(configPath))
            {
                configPath = DefaultConfigPath;
            }
            else
            {
                // Если переданный путь существует как папка, добавляем appsettings.json
                if (Directory.Exists(configPath))
                {
                    configPath = Path.Combine(configPath, "appsettings.json");
                }
                // Если переданный путь существует как файл — оставляем как есть
                // Если не существует — оставляем, и тогда ниже будет ошибка "файл не найден"
            }

            // 2. Загружаем JSON (если файл существует)
            AppConfig jsonConfig = null;
            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    jsonConfig = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });


                    logger.Info($"[AppConfig] Загружена конфигурация из {configPath}");
                }
                catch (Exception ex)
                {
                    logger.Error($"[AppConfig] Ошибка чтения JSON: {ex.Message}");
                }
            }
            else
            {
                logger.Warn($"[AppConfig] Файл {configPath} не найден, используются hardcoded значения");
            }

            // 3. Проверяем, заданы ли какие-либо параметры командной строки (кроме --config)
            bool hasAnyCliOption = cmdOptions != null && (
                !string.IsNullOrEmpty(cmdOptions.Format) ||
                !string.IsNullOrEmpty(cmdOptions.DefaultFolderPath) ||
                !string.IsNullOrEmpty(cmdOptions.UserFolderPath) ||
                (cmdOptions.EnabledApplications?.Any() == true) ||
                (cmdOptions.Flags?.Any() == true)
            );

            // 4. Если параметры CLI отсутствуют → возвращаем то, что загружено из JSON
            if (!hasAnyCliOption)
            {
                if (jsonConfig == null)
                {
                    // Нет ни JSON, ни CLI → создаём новый объект (применяются hardcoded инициализаторы)
                    jsonConfig = new AppConfig();
                    logger.Info("[AppConfig] Нет конфигурации → используются встроенные значения по умолчанию");
                }
                return jsonConfig;
            }

            // 5. Есть параметры CLI → начинаем с hardcoded, затем накладываем JSON, затем CLI
            AppConfig finalConfig = new AppConfig();

            if (!string.IsNullOrEmpty(cmdOptions.UserFolderPath))
                finalConfig.UserFolderPath = cmdOptions.UserFolderPath.Trim();


            if (cmdOptions.DaysToCollect.HasValue)
                finalConfig.DaysToCollect = cmdOptions.DaysToCollect.Value;


            // Накладываем JSON (если есть)
            if (jsonConfig != null)
            {
                if (!string.IsNullOrEmpty(jsonConfig.DefaultFolderPath)) finalConfig.DefaultFolderPath = jsonConfig.DefaultFolderPath;
                if (!string.IsNullOrEmpty(jsonConfig.Format)) finalConfig.Format = jsonConfig.Format;
                if (!string.IsNullOrEmpty(jsonConfig.UserFolderPath)) finalConfig.UserFolderPath = jsonConfig.UserFolderPath;
                if (jsonConfig.EnabledApplications != null) finalConfig.EnabledApplications = jsonConfig.EnabledApplications;
                if (jsonConfig.Flags != null) finalConfig.Flags = jsonConfig.Flags;
                if (jsonConfig.DaysToCollect != 0) finalConfig.DaysToCollect = jsonConfig.DaysToCollect;
            }

            // Накладываем CLI (высший приоритет)
            if (!string.IsNullOrEmpty(cmdOptions.Format)) finalConfig.Format = cmdOptions.Format;
            if (!string.IsNullOrEmpty(cmdOptions.DefaultFolderPath)) finalConfig.DefaultFolderPath = cmdOptions.DefaultFolderPath;
            if (!string.IsNullOrEmpty(cmdOptions.UserFolderPath)) finalConfig.UserFolderPath = cmdOptions.UserFolderPath;
            if (cmdOptions.DaysToCollect.HasValue) finalConfig.DaysToCollect = cmdOptions.DaysToCollect.Value;




            if (cmdOptions.EnabledApplications?.Any() == true)
            { 
                finalConfig.EnabledApplications = cmdOptions.EnabledApplications
                    .Select(appName => new AppConfig { Name = appName }) // Укажите ваше свойство вместо Name
                    .ToArray();
            }

            if (cmdOptions.Flags?.Any() == true) finalConfig.Flags = cmdOptions.Flags.ToArray();

            return finalConfig;
        }

        // --- Сохранение конфигурации в указанный файл (по умолчанию в стандартный) ---
        public void Save(string filePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
                filePath = DefaultConfigPath;

            try
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
                logger.Info($"[AppConfig] Конфигурация сохранена в {filePath}");
            }
            catch (Exception ex)
            {
                logger.Error($"[AppConfig] Ошибка сохранения конфигурации: {ex.Message}");
            }
        }
    }
}