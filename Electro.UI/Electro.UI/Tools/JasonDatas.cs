using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electro.UI.Tools
{

    public class Rootobject
    {
        public string lastVersion { get; set; }
        public string downloadPath { get; set; }
        public Dns dns { get; set; }
    }

    public class Dns
    {
        public string[] electro { get; set; }
        public string[] google { get; set; }
    }



    public class Version
    {
        public string lastVersion { get; set; }
        public string downloadPath { get; set; }
    }

}
