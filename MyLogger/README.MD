MyCompany.Logging
Библиотека для файлового логирования с поддержкой ротации, очистки старых логов и маскировки конфиденциальных данных.

🚀 Быстрый старт
1. Установка
Добавьте ссылку на сборку MyCompany.Logging в ваш проект.

2. Настройка и создание логгера
csharp
using MyCompany.Logging;

// Параметры конфигурации
var options = new LoggingOptions
{
    LogDirectory = "Logs",              // Папка для логов (абсолютный или относительный путь)
    MinLevel = LogLevel.Info,           // Минимальный уровень логирования
    MaxFileSizeMB = 10,                 // Максимальный размер файла (МБ)
    RetentionDays = 30,                 // Сколько дней хранить логи
    MaskSensitiveData = true            // Маскировать чувствительные данные
};

// Создаём экземпляр логгера
ILogger logger = new FileLogger(options);
// или используя фабричный метод:
// ILogger logger = LogManager.CreateFileLogger(options);
3. Запись сообщений
csharp
logger.Debug("Отладочное сообщение");
logger.Info("Информационное сообщение", "MyCategory");
logger.Warn("Предупреждение");
logger.Error("Ошибка");
logger.Error(ex, "Сообщение об ошибке", "Category");
4. Маскирование данных (по умолчанию)
Если MaskSensitiveData = true, автоматически маскируются:

Токены – длинные hex-строки (>8 символов) заменяются на abcd***

IP-адреса – маскируются последние две октеты (192.168.*.*)

Пароли – значения после password= или pwd= заменяются на ***

Пути к файлам – оставляется только корень диска (C:\***)

Можно реализовать свой маскер через интерфейс IDataMasker и передать его в конструктор.

📦 Интеграция с DI (Microsoft.Extensions.DependencyInjection)
Добавьте метод расширения для регистрации:

csharp
public static IServiceCollection AddFileLogging(this IServiceCollection services, Action<LoggingOptions> configure)
{
    services.AddSingleton<ILogger>(sp =>
    {
        var options = new LoggingOptions();
        configure(options);
        return new FileLogger(options);
    });
    return services;
}
Использование:

csharp
services.AddFileLogging(options =>
{
    options.LogDirectory = "C:\\MyApp\\Logs";
    options.MinLevel = LogLevel.Debug;
});
🔧 Ключевые классы и интерфейсы
Тип	Назначение
ILogger	Основной интерфейс для логирования.
FileLogger	Реализация, пишущая в файлы с ротацией и очисткой.
LoggingOptions	Параметры конфигурации (путь, уровни, размеры, срок хранения).
IDataMasker	Интерфейс для маскировки данных.
DefaultDataMasker	Стандартный маскер (токены, IP, пароли, пути).
🗂️ Формат логов
Каждая запись имеет вид:

text
[2026-03-23 10:15:30.123] [Info] [General] Сообщение
При ошибке добавляется стектрейс:

text
[2026-03-23 10:15:30.456] [Error] [Database] Connection failed
System.Data.SqlClient.SqlException: ...
🔄 Ротация файлов
Файлы именуются по дате: 2026-03-23.log

При превышении MaxFileSizeMB файл переименовывается в 2026-03-23.log.1, существующие .1→.2 и т.д. (максимум 9 архивов).

Старые архивы удаляются по истечении RetentionDays.

🧹 Очистка
Фоновая задача запускается раз в сутки и удаляет файлы старше RetentionDays дней.

⚠️ Примечания
Логгер реализует IDisposable – вызывайте Dispose() при завершении работы приложения, чтобы остановить фоновую очистку.

Все операции записи синхронны и блокируются на уровне файла (блокировка lock). Для высоконагруженных сценариев рекомендуется добавить асинхронную очередь.

Исключения при очистке игнорируются, чтобы не нарушать работу приложения.

