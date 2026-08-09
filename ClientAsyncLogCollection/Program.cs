using ClientAsyncLogCollection.core.LogRequest;
using ClientAsyncLogCollection.core.SearchWordPlanerTasks;
using ClientAsyncLogCollection.Utils.Security.SaveConf;
using CommandLine;
using System.Threading.Tasks;

namespace ClientAsyncLogCollection
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            AppConfig.CommandLineOptions cmdOptions = null;

            Parser.Default.ParseArguments<AppConfig.CommandLineOptions>(args)
                .WithParsed(opts => cmdOptions = opts)
                .WithNotParsed(errors => Environment.Exit(1));

            if (cmdOptions == null) return;

            // Единый вызов
            AppConfig config = AppConfig.Load(cmdOptions);


            var cache = new ProviderCache();

            var scanner = new ProviderScanner();
            var mapper = new ProviderMapper(cache, scanner);

            var providerMap = await mapper.GetMappingAsync(config, forceRefresh: config.ForceProviderRefresh);

            await LogRequest.GetLogAsync(config, providerMap);
        }
    }
}
