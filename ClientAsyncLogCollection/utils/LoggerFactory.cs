using MyCompany.Logging;

namespace ClientAsyncLogCollection.Utils
{
    public static class LoggerFactory
    {
        private static readonly LoggingOptions Options = new LoggingOptions();
        private static readonly FileLogger Instance = new FileLogger(Options);

        public static FileLogger Logger => Instance;
    }
}