using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace ClientAsyncLogCollection.core.SearchWordPlanerTasks
{
    public interface IProviderCache
    {
        Task<Dictionary<string, (string RealName, string LogName)>> LoadAsync();
        Task SaveAsync(Dictionary<string, (string RealName, string LogName)> mapping);
        bool IsStale { get; }
    }
}
