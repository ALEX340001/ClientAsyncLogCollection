using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientAsyncLogCollection.utils
{
    internal class GetLogNameForProvider
    {
        public string Call(string providerName)
        {
            try
            {
                using (var metadata = new ProviderMetadata(providerName))
                {
                    var logLink = metadata.LogLinks.FirstOrDefault();
                    if (logLink != null)
                    {
                        return logLink.LogName;
                    }
                }
            }
            catch
            {
                // Если источник не найден в реестре Windows (например, опечатка)
            }

            // Значение по умолчанию, если ОС ничего не знает об этом имени
            return null;
        }

    }
}
