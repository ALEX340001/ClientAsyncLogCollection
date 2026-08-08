# ClientAsyncLogCollection

**Асинхронный сбор событий из журналов Windows** с гибкой настройкой, кэшированием провайдеров и экспортом в JSON/XML.

---

## Возможности

- Асинхронное чтение событий из журналов `Application`, `System`, `Security` и пользовательских журналов.
- Автоматическое сопоставление имён провайдеров с реальными журналами (с кэшированием для ускорения повторных запусков).
- Гибкая конфигурация через JSON-файл или параметры командной строки (CLI-параметры имеют приоритет).
- Экспорт результатов в **JSON** или **XML** с автоматическим созданием папок по имени приложения.
- Встроенная система логирования с ротацией файлов и маскировкой чувствительных данных (IP-адреса, токены, пароли, пути).
- Валидация ввода для предотвращения инъекций.
- Веб-генератор команд и конфигурации (в папке `GitWebSite`).

---

## Системные требования

- **.NET 8.0** или выше
- **Windows** (используется `System.Diagnostics.EventLog`)
- Права на чтение журналов событий (обычно требуются права администратора для некоторых журналов)

---

## Установка и сборка

1. Клонируйте репозиторий:
   ```bash
   git clone https://github.com/ALEX340001/ClientAsyncLogCollection.git
   cd ClientAsyncLogCollection
   ```

2. Соберите проект:
   ```bash
   dotnet build -c Release
   ```

3. Запустите (пример):
   ```bash
   dotnet run --project ClientAsyncLogCollection/ClientAsyncLogCollection.csproj -- --apps "Service Control Manager,DistributedCOM" -t 1
   ```

Или запустите собранный исполняемый файл из папки `bin/Release/net8.0/`.

---

## Конфигурация

Основной файл конфигурации — `appsettings.json` (в папке `ClientAsyncLogCollection`). Пример:

```json
{
  "Domain": "corp.groupmegapolis.ru",
  "Format": "Json",
  "DefaultFolderPath": "C:\\Logs",
  "UserFolderPath": "",
  "EnabledApplications": [
    { "Name": "Service Control Manager", "Log": "System" },
    { "Name": "DistributedCOM", "Log": "Application" }
  ],
  "Flags": [1, 2, 3]
}
```

Параметры командной строки имеют приоритет над настройками из JSON.

---

## Использование (CLI)

```bash
ClientAsyncLogCollection.exe [options]
```

| Опция | Описание |
|-------|----------|
| `-c, --config <path>` | Путь к JSON-конфигу (по умолчанию `appsettings.json`) |
| `-t, --days <number>` | За сколько дней собрать логи (по умолчанию 1) |
| `-f, --format <json/xml>` | Формат вывода (по умолчанию `json`) |
| `--defaultFolder <path>` | Папка по умолчанию для сохранения логов |
| `--pl <path>` | Пользовательская папка для сохранения логов (переопределяет `defaultFolder`) |
| `--apps <list>` | Список имён приложений (провайдеров) через запятую |
| `--flags <list>` | Список числовых флагов (для расширения функциональности) |

**Примеры:**

```bash
# Базовый запуск с конфигом по умолчанию
ClientAsyncLogCollection.exe

# Сбор за 2 дня, экспорт в XML, пользовательская папка
ClientAsyncLogCollection.exe -t 2 -f xml --pl "C:\Reports"

# Указание провайдеров вручную
ClientAsyncLogCollection.exe --apps "Service Control Manager,DistributedCOM" --defaultFolder "C:\Logs"
```

---

## Архитектура проекта

Проект разделён на логические слои:

- **`Program.cs`** — точка входа, парсинг аргументов командной строки.
- **`core/LogRequest`** — асинхронный запрос событий и форматирование (JSON/XML).
- **`core/SearchWordPlannerTasks`** — маппинг провайдеров с кэшированием (кэш хранится в `%LocalAppData%\ClientAsyncLogCollection\`).
- **`utils`** — валидация ввода, логирование, загрузка конфигурации.
- **`MyLogger`** — переиспользуемая библиотека логирования с маскировкой данных и ротацией файлов.
- **`GitWebSite`** — веб-генератор команд и конфигурации (можно развернуть на GitHub Pages).

Подробнее см. в документации на [GitHub Pages](https://ALEX340001.github.io/ClientAsyncLogCollection) (если будет развёрнута).

---

## Лицензия

Проект распространяется под лицензией MIT. Подробности в файле [LICENSE](LICENSE) (если добавите).

---

## Вклад

Если вы нашли ошибку или хотите предложить улучшение — создайте Issue или Pull Request.

---

## Контакты

Автор: [ALEX340001](https://github.com/ALEX340001)

---

**Приятного использования!**
