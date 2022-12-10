using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Electro.UI.Tools.WireGuardTunnel
{
    public class Keypair
    {
        public readonly string Public;
        public readonly string Private;

        public Keypair(string pub, string priv)
        {
            Public = pub;
            Private = priv;
        }

        [DllImport("tunnel.dll", EntryPoint = "WireGuardGenerateKeypair", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool WireGuardGenerateKeypair(byte[] publicKey, byte[] privateKey);

        public static Keypair Generate()
        {
            var publicKey = new byte[32];
            var privateKey = new byte[32];
            WireGuardGenerateKeypair(publicKey, privateKey);
            return new Keypair(Convert.ToBase64String(publicKey), Convert.ToBase64String(privateKey));
        }
    }
}
