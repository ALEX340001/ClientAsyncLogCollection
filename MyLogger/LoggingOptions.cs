namespace MyCompany.Logging
{
    /// <summary>
    /// Уровни логирования.
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error
    }

    /// <summary>
    /// Параметры конфигурации логгера.
    /// </summary>
    public class LoggingOptions
    {
        private static string _currentLogFile;
        public string LogDirectory { get; set; } = GetCurrentLogFile();
        public int MaxFileSizeMB { get; set; } = 10;
        public int RetentionDays { get; set; } = 30;
        public bool MaskSensitiveData { get; set; } = false;
        public LogLevel MinLevel { get; set; } = LogLevel.Info;
    
        

      /// <summary>
        /// Получение пути к текущему лог-файлу
        /// </summary>
        private static string GetCurrentLogFile()
        {
            if (_currentLogFile == null)
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string logFolderPath = Path.Combine(documentsPath, "log_app_ClientAsyncLogCollection");

                if (!Directory.Exists(logFolderPath))
                {
                    Directory.CreateDirectory(logFolderPath);
                }

                string dateTimeString = DateTime.Now.ToString("yyyy-MM-dd");
                _currentLogFile = Path.Combine(logFolderPath, $"{dateTimeString}_log.txt");
            }

            return _currentLogFile;
        }
    

        /// <summary>
        /// Ротация лог-файла при превышении размера
        /// </summary>
    }
}