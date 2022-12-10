using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electro.UI.Tools
{
    internal sealed class MyLogger
    {
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private static MyLogger _instance;
        private MyLogger() { }

        public static MyLogger GetInstance()
        {
            if (_instance == null)
                _instance = new MyLogger();
            return _instance;
        }

        public NLog.Logger Logger
        {
            get => logger;
            set => logger = value;
        }
    }
}
