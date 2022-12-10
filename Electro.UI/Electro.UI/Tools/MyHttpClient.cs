using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Electro.UI.Tools
{
    public sealed class MyHttpClient
    {
        private static MyHttpClient _instance;
        private HttpClient client = new HttpClient();

        private MyHttpClient() { }

        public static MyHttpClient GetInstance()
        {
            if (_instance == null)
            {
                _instance = new MyHttpClient();
            }
            return _instance;
        }

        public HttpClient Client
        {
            get => client;
            set => client = value;
        }
    }
}
