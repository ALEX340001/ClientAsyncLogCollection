using ClientAsyncLogCollection.Utils;
using MyCompany.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ClientAsyncLogCollection.core.SearchWordPlanerTasks
{
    public class ProviderCache : IProviderCache
    {
        private static readonly FileLogger logger = LoggerFactory.Logger;

        private readonly string _cachePath;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly int _maxAgeDays;

        public ProviderCache(string cacheFolder = null, int maxAgeDays = 30)
        {
            var folder = cacheFolder ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ClientAsyncLogCollection");
            _cachePath = Path.Combine(folder, "provider_cache.json");
            _maxAgeDays = maxAgeDays;

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                logger.Info($"[ProviderCache] Создана директория для кэша: {folder}");
            }

            logger.Info($"[ProviderCache] Инициализирован кэш. Путь: {_cachePath}, Макс. возраст: {_maxAgeDays} дней");
        }

        public bool IsStale
        {
            get
            {
                var fi = new FileInfo(_cachePath);
                if (!fi.Exists)
                {
                    logger.Info("[ProviderCache] Кэш устарел: файл не существует");
                    return true;
                }

                var ageDays = (DateTime.UtcNow - fi.LastWriteTimeUtc).TotalDays;
                bool isStale = ageDays > _maxAgeDays;

                if (isStale)
                {
                    logger.Info($"[ProviderCache] Кэш устарел: возраст {ageDays:F1} дней, лимит {_maxAgeDays} дней");
                }

                return isStale;
            }
        }

        public async Task<Dictionary<string, (string RealName, string LogName)>> LoadAsync()
        {
            logger.Info("[ProviderCache] Попытка загрузки кэша");
            await _lock.WaitAsync();
            try
            {
                if (!File.Exists(_cachePath))
                {
                    logger.Warn($"[ProviderCache] Файл кэша не найден по пути: {_cachePath}");
                    return null;
                }

                var json = await File.ReadAllTextAsync(_cachePath);
                var raw = JsonSerializer.Deserialize<CacheFile>(json);
                if (raw == null)
                {
                    logger.Warn("[ProviderCache] Файл кэша пуст или некорректен после десериализации");
                    return null;
                }

                var dict = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);
                foreach (var entry in raw.Providers)
                {
                    dict[entry.Requested] = (entry.RealName, entry.LogName);
                }

                logger.Info($"[ProviderCache] Успешно загружено {dict.Count} записей из кэша. Дата генерации кэша: {raw.GeneratedAt}");
                return dict;
            }
            catch (Exception ex)
            {
                logger.Error($"[ProviderCache] Ошибка при чтении или десериализации кэша: {ex.Message}");
                return null;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SaveAsync(Dictionary<string, (string RealName, string LogName)> mapping)
        {
            if (mapping == null)
            {
                logger.Warn("[ProviderCache] Попытка сохранить пустой (null) маппинг в кэш. Действие отменено");
                return;
            }

            logger.Info($"[ProviderCache] Попытка сохранения кэша. Количество записей: {mapping.Count}");
            await _lock.WaitAsync();
            try
            {
                var cache = new CacheFile
                {
                    GeneratedAt = DateTime.UtcNow,
                    ScanHours = 168,
                    Providers = mapping.Select(kvp => new CacheEntry
                    {
                        Requested = kvp.Key,
                        RealName = kvp.Value.RealName,
                        LogName = kvp.Value.LogName
                    }).ToList()
                };

                var json = JsonSerializer.Serialize(cache, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_cachePath, json);

                logger.Info($"[ProviderCache] Кэш успешно сохранен в файл: {_cachePath}");
            }
            catch (Exception ex)
            {
                logger.Error($"[ProviderCache] Ошибка при сериализации или записи кэша: {ex.Message}");
            }
            finally
            {
                _lock.Release();
            }
        }

        // Внутренние классы для сериализации
        private class CacheFile
        {
            public DateTime GeneratedAt { get; set; }
            public int ScanHours { get; set; }
            public List<CacheEntry> Providers { get; set; }
        }
        private class CacheEntry
        {
            public string Requested { get; set; }
            public string RealName { get; set; }
            public string LogName { get; set; }
        }
    }
}
