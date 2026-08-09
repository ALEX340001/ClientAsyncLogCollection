using ClientAsyncLogCollection.Utils;
using ClientAsyncLogCollection.Utils.Security.SaveConf;
using MyCompany.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientAsyncLogCollection.core.SearchWordPlanerTasks
{
    public class ProviderMapper
    {


        private static readonly FileLogger logger = LoggerFactory.Logger;

        private readonly IProviderCache _cache;
        private readonly IProviderScanner _scanner;

        public ProviderMapper(IProviderCache cache, IProviderScanner scanner)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
        }

        public async Task<Dictionary<string, (string RealName, string LogName)>> GetMappingAsync(
            AppConfig config,
            bool forceRefresh = false)
        {
            var requestedNames = config.EnabledApplications?.Select(a => a.Name).ToList() ?? new List<string>();
            int scanHours = config.ScanHours > 0 ? config.ScanHours : 168;

            // Пытаемся загрузить из кэша, если не принудительно
            if (!forceRefresh && !_cache.IsStale)
            {
                var cached = await _cache.LoadAsync();
                if (cached != null)
                {
                    var missing = requestedNames.Except(cached.Keys, StringComparer.OrdinalIgnoreCase).ToList();
                    if (!missing.Any())
                    {
                        ProviderMapper.logger.Info("Используем существующий кэш провайдеров.");
                        return cached;
                    }
                    ProviderMapper.logger.Warn($"В кэше отсутствуют: {string.Join(", ", missing)}. Выполняем сканирование.");
                }
        }

            logger?.Info($"Сканирование журналов за последние {scanHours} часов...");
            var realProviders = await _scanner.ScanAsync(scanHours);
            var mapping = BuildMapping(requestedNames, realProviders, scanHours);
            await _cache.SaveAsync(mapping);
            return mapping;
        }

        private Dictionary<string, (string RealName, string LogName)> BuildMapping(
            IEnumerable<string> requestedNames,
            Dictionary<string, (string RealName, string LogName)> realProviders,
            int scanHours)
        {
            var mapping = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);
            foreach (var requested in requestedNames)
            {
                // 1. Точное совпадение
                if (realProviders.TryGetValue(requested, out var exact))
                {
                    mapping[requested] = exact;
                    ProviderMapper.logger.Info($"Точное совпадение: '{requested}' -> '{exact.RealName}' (журнал {exact.LogName})");
                    continue;
                }

                // 2. Собираем всех кандидатов для частичного совпадения
                var candidates = new List<(string Name, (string RealName, string LogName) Data, int Score)>();
                foreach (var kv in realProviders)
                {
                    string real = kv.Key;
                    // Простейшая оценка: длина наибольшей общей подстроки
                    int commonSubstringLen = LongestCommonSubstringLength(requested, real);
                    if (commonSubstringLen > 0)
                    {
                        // Доп. бонус, если requested начинается с real или наоборот
                        int bonus = 0;
                        if (real.StartsWith(requested, StringComparison.OrdinalIgnoreCase) ||
                            requested.StartsWith(real, StringComparison.OrdinalIgnoreCase))
                            bonus = 100;
                        int score = commonSubstringLen + bonus;
                        candidates.Add((real, kv.Value, score));
                    }
                }

                if (candidates.Any())
                {
                    // Сортируем по убыванию оценки, выбираем лучший
                    var best = candidates.OrderByDescending(c => c.Score).First();
                    mapping[requested] = best.Data;
                    ProviderMapper.logger.Info($"Частичное совпадение (оценка {best.Score}): '{requested}' -> '{best.Name}' (журнал {best.Data.LogName})");
                    continue;
                }

                // 3. Fallback по первой букве (можно вообще убрать)
                if (requested.Length > 1)
                {
                    var firstLetter = requested[0].ToString();
                    var firstLetterMatch = realProviders.FirstOrDefault(p =>
                        p.Key.StartsWith(firstLetter, StringComparison.OrdinalIgnoreCase));
                    if (firstLetterMatch.Key != null)
                    {
                        mapping[requested] = firstLetterMatch.Value;
                        ProviderMapper.logger.Warn($"Для '{requested}' не найдено хороших совпадений. Используем первый на букву '{firstLetter}': '{firstLetterMatch.Key}'");
                        continue;
                    }
                }

                ProviderMapper.logger.Warn($"Не удалось сопоставить '{requested}' ни с одним провайдером за последние {scanHours} часов.");
            }
            return mapping;
        }

        // Вспомогательный метод: длина наибольшей общей подстроки (без учёта регистра)
        private static int LongestCommonSubstringLength(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0;
            a = a.ToLowerInvariant();
            b = b.ToLowerInvariant();
            int[,] dp = new int[a.Length, b.Length];
            int maxLen = 0;
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < b.Length; j++)
                {
                    if (a[i] == b[j])
                    {
                        if (i == 0 || j == 0) dp[i, j] = 1;
                        else dp[i, j] = dp[i - 1, j - 1] + 1;
                        if (dp[i, j] > maxLen) maxLen = dp[i, j];
                    }
                    else dp[i, j] = 0;
                }
            }
            return maxLen;
        }
    }
}
