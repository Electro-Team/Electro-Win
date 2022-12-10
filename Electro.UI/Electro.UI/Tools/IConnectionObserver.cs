using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electro.UI.Tools
{
    public interface IConnectionObserver
    {
        bool IsGettingData { get; set; }

        void ConnectionObserver(bool? SuccessfullyCoonected, string ServiceText);
    }
}
