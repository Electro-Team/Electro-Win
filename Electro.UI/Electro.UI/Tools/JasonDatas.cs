using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electro.UI.Tools
{
    public class JasonDatas
    {
        public class Electro
        {
            public string DNS1 { get; set; }
            public string DNS2 { get; set; }
        }

        public class Google
        {
            public string DNS1 { get; set; }
            public string DNS2 { get; set; }
        }

        public class Dns
        {
            public List<Electro> electro { get; set; }
            public List<Google> google { get; set; }
        }

        public class Ovpn
        {
            public string profile { get; set; }
            public string user { get; set; }
            public string pass { get; set; }
        }

        public class Root
        {
            public string status { get; set; }
            public string version { get; set; }
            public Dns dns { get; set; }
            public Ovpn ovpn { get; set; }
        }
    }
}
