using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientAsyncLogCollection.core.SearchWordPlanerTasks
{
        public record ProviderMappingResult(
            string RequestedName,
            string RealName,
            string LogName
        );
}
