using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientAsyncLogCollection.core.SearchWordPlanerTasks
{
        public interface IProviderScanner
        {
            Task<Dictionary<string, (string RealName, string LogName)>> ScanAsync(int scanHours);
        }
    }
