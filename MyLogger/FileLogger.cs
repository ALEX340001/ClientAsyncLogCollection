using System;
using System.IO;
using System.Threading;

namespace MyCompany.Logging
{
    /// <summary>
    /// Реализация логгера, пишущего в файлы с поддержкой ротации и очистки старых логов.
    /// </summary>
    public class FileLogger : ILogger, IDisposable
    {
        private readonly string _logDirectory;
        private readonly LogLevel _minLevel;
        private readonly IDataMasker _masker;
        private readonly bool _maskSensitiveData;
        private readonly int _maxFileSizeMB;
        private readonly int _retentionDays;
        private readonly object _lock = new object();
        private Timer _cleanupTimer;

        /// <summary>
        /// Инициализирует новый экземпляр логгера.
        /// </summary>
        /// <param name="options">Параметры конфигурации логгирования.</param>
        /// <param name="masker">Опциональный маскер конфиденциальных данных. Если не указан, используется <see cref="DefaultDataMasker"/>.</param>
        /// <exception cref="ArgumentNullException">Если options равен null.</exception>
        public FileLogger(LoggingOptions options, IDataMasker masker = null)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _logDirectory = Path.IsPathRooted(options.LogDirectory)
                ? options.LogDirectory
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, options.LogDirectory);
            Directory.CreateDirectory(_logDirectory);

            _minLevel = options.MinLevel;
            _masker = masker ?? new DefaultDataMasker();
            _maskSensitiveData = options.MaskSensitiveData;
            _maxFileSizeMB = options.MaxFileSizeMB;
            _retentionDays = options.RetentionDays;

            StartCleanupTask();
        }

        #region ILogger implementation

        public void Debug(string message, string category = null) => Log(LogLevel.Debug, message, null, category);
        public void Info(string message, string category = null) => Log(LogLevel.Info, message, null, category);
        public void Warn(string message, string category = null) => Log(LogLevel.Warn, message, null, category);
        public void Error(string message, string category = null) => Log(LogLevel.Error, message, null, category);
        public void Error(Exception ex, string message, string category = null) => Log(LogLevel.Error, message, ex, category);

        #endregion

        #region Private methods

        private void Log(LogLevel level, string message, Exception ex = null, string category = null)
        {
            if (level < _minLevel) return;

            var fullMessage = FormatMessage(level, message, ex, category);
            if (_maskSensitiveData)
                fullMessage = _masker.Mask(fullMessage);

            WriteToFile(fullMessage);
        }

        private string FormatMessage(LogLevel level, string message, Exception ex, string category)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var cat = string.IsNullOrEmpty(category) ? "General" : category;
            var exPart = ex != null ? $"\n{ex}" : "";
            return $"[{timestamp}] [{level}] [{cat}] {message}{exPart}";
        }

        private void WriteToFile(string line)
        {
            lock (_lock)
            {
                var date = DateTime.Now.ToString("yyyy-MM-dd");
                var filePath = Path.Combine(_logDirectory, $"{date}.log");
                var fileInfo = new FileInfo(filePath);

                if (fileInfo.Exists && fileInfo.Length > _maxFileSizeMB * 1024L * 1024L)
                {
                    RotateLogFile(filePath);
                }

                File.AppendAllText(filePath, line + Environment.NewLine);
            }
        }

        private void RotateLogFile(string filePath)
        {
            if (File.Exists(filePath + ".9"))
                File.Delete(filePath + ".9");

            for (int i = 8; i >= 1; i--)
            {
                string oldFile = filePath + "." + i;
                string newFile = filePath + "." + (i + 1);
                if (File.Exists(oldFile))
                    File.Move(oldFile, newFile, true);
            }

            File.Move(filePath, filePath + ".1");
        }

        private void StartCleanupTask()
        {
            _cleanupTimer = new Timer(_ => CleanupOldLogs(), null, TimeSpan.FromHours(24), TimeSpan.FromHours(24));
        }

        private void CleanupOldLogs()
        {
            try
            {
                var cutoff = DateTime.Now.AddDays(-_retentionDays);
                foreach (var file in Directory.GetFiles(_logDirectory, "*.log*"))
                {
                    var fi = new FileInfo(file);
                    if (fi.CreationTime < cutoff)
                        fi.Delete();
                }
            }
            catch
            {
                // Ошибки очистки не должны влиять на работу приложения.
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
        }

        #endregion
    }
}