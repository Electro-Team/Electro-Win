using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electro.UI.Interfaces
{
    public interface IDNSService
    {
        Task GetDataFromServerAndSetDNS();
        Task<bool> Connect();
        Task UnsetDNS();
        void Dispose();
    }
}
