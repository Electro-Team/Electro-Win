using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electro.UI.Tools
{
    interface IConnectionObserver
    {
        void ConnectionObserver(bool? SuccessfullyCoonected, string ServiceText);
    }
}
