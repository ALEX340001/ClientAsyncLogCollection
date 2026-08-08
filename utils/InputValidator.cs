using MyCompany.Logging;
using System;
using System.Text.RegularExpressions;
namespace ClientAsyncLogCollection.Utils
{
    /// Валидатор входных данных для предотвращения инъекций
    public static class InputValidator
    {
        // --- Логгер  ---
        private static readonly FileLogger logger = LoggerFactory.Logger;

        // Regex для валидации имени компьютера (NetBIOS или DNS)
        private static readonly Regex HostNameRegex = new Regex(
            @"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        // Regex для валидации имени пользователя (домен\пользователь или просто пользователь)
        private static readonly Regex UserNameRegex = new Regex(
            @"^[a-zA-Z0-9._\-\\]+$",
            RegexOptions.Compiled
        );



        // Опасные символы для command injection
        private static readonly char[] DangerousChars = { ';', '|', '&', '>', '<', '`', '$', '(', ')', '{', '}', '\n', '\r' };

        /// Валидация имени компьютера
        public static bool IsValidComputerName(string computerName)
        {
            if (string.IsNullOrWhiteSpace(computerName))
            {
                logger.Info("[InputValidator]  Имя компьютера пустое"); 
                return false;
            }

            if (computerName.Length > 253)
            {
                logger.Info($"[InputValidator]  Имя компьютера слишком длинное: {computerName.Length} символов"); 
                return false;
            }

            if (computerName.IndexOfAny(DangerousChars) != -1)
            {
                logger.Info($"[InputValidator]  Имя компьютера содержит опасные символы: {computerName}");  // ← Изменили
                return false;
            }

            if (!HostNameRegex.IsMatch(computerName))
            {
                logger.Info($"[InputValidator]  Имя компьютера не соответствует формату: {computerName}");  // ← Изменили
                return false;
            }

            logger.Info($"[InputValidator]  Имя компьютера валидно: {computerName}");  
            return true;
        }

        public static bool IsValidUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                logger.Info("[InputValidator]  Имя пользователя пустое");  
                return false;
            }

            if (userName.Length > 256)
            {
                logger.Info($"[InputValidator]  Имя пользователя слишком длинное: {userName.Length} символов");  
                return false;
            }

            if (userName.IndexOfAny(DangerousChars) != -1)
            {
                logger.Info($"[InputValidator]  Имя пользователя содержит опасные символы: {userName}");  
                return false;
            }

            if (!UserNameRegex.IsMatch(userName))
            {
                logger.Info($"[InputValidator]  Имя пользователя содержит недопустимые символы: {userName}");  // ← Изменили
                return false;
            }

            logger.Info($"[InputValidator]  Имя пользователя валидно: {userName}");  // ← Изменили
            return true;
        }

        /// Санитизация строки для логирования (маскирование чувствительных данных)
        public static string SanitizeForLog(string input, bool maskCompletely = false)
        {
            if (string.IsNullOrEmpty(input))
                return "<empty>";

            if (maskCompletely)
                return "***";

            // Показываем только первые 3 символа
            if (input.Length <= 3)
                return "***";

            return input.Substring(0, 3) + "***";
        }
    }
}
