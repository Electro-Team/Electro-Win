using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Electro.UI.Interfaces;
using Electro.UI.Tools;

namespace Electro.UI.Services
{
    public class WireGuard : IService
    {
        private IConnectionObserver connectionObserver;

        public string ServiceText { get; }

        internal WireGuard(IConnectionObserver connectionObserver)
        {
            this.connectionObserver = connectionObserver;
        }
        public Task<bool> Connect()
        {
            throw new NotImplementedException();
        }

        public async Task Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
