using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electro.UI.Services
{
    internal interface IService
    {
        string ServiceText { get;}
        Task<bool> Connect();
        void Dispose();
    }
}
