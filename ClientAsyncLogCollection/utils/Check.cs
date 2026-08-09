using ClientAsyncLogCollection.Utils;
using MyCompany.Logging;
using System;

namespace ClientAsyncLogCollection.Utils.Security

{
    internal class Check
    {
        // --- Логгер  ---
        private static readonly FileLogger logger = LoggerFactory.Logger;
       
        /// Чтение и валидация имени компьютера
        
        public static string ReadComputerName(string message)
        {
            logger.Info("[Check] Запрос имени компьютера");

            int attemptCount = 0;
            const int maxAttempts = 5;

            while (attemptCount < maxAttempts)
            {
                attemptCount++;
                Console.WriteLine(message);
                string input = Console.ReadLine()?.Trim();

                logger.Info($"[Check] Попытка {attemptCount}/{maxAttempts}: получен ввод");

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine(" Пустой ввод. Попробуйте еще раз.");
                    logger.Info($"[Check] Попытка {attemptCount}: пустой ввод");
                    continue;
                }

                if (!InputValidator.IsValidComputerName(input))
                {
                    Console.WriteLine(" Невалидное имя компьютера. Используйте только буквы, цифры, дефис и точку.");
                    logger.Info($"[Check] Попытка {attemptCount}: невалидное имя");
                    continue;
                }

                logger.Info($"[Check]  Валидное имя компьютера получено");
                return input;
            }

            logger.Info($"[Check]  Превышено максимальное количество попыток ({maxAttempts})");
            throw new InvalidOperationException($"Превышено максимальное количество попыток ввода ({maxAttempts})");
        }

        /// Чтение и валидация имени пользователя
        public static string ReadUserName(string message)
        {
            logger.Info("[Check] Запрос имени пользователя");

            int attemptCount = 0;
            const int maxAttempts = 5;

            while (attemptCount < maxAttempts)
            {
                attemptCount++;
                Console.WriteLine(message);
                string input = Console.ReadLine()?.Trim();

                logger.Info($"[Check] Попытка {attemptCount}/{maxAttempts}: получен ввод");

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine(" Пустой ввод. Попробуйте еще раз.");
                    logger.Info($"[Check] Попытка {attemptCount}: пустой ввод");
                    continue;
                }

                if (!InputValidator.IsValidUserName(input))
                {
                    Console.WriteLine(" Невалидное имя пользователя. Используйте только буквы, цифры, точку, дефис и обратный слэш.");
                    logger.Info($"[Check] Попытка {attemptCount}: невалидное имя пользователя");
                    continue;
                }

                logger.Info($"[Check]  Валидное имя пользователя получено");
                return input;
            }

            logger.Info($"[Check]  Превышено максимальное количество попыток ({maxAttempts})");
            throw new InvalidOperationException($"Превышено максимальное количество попыток ввода ({maxAttempts})");
        }
    }
}
