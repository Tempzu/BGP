using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BGP_Router
{
    class Router
    {
        public Socket listenerSocket;
        public Socket speakerSocket;
        public byte[] buffer = new byte[1024];

        private static ResetEvent speakerStarted = new ResetEvent(true);



    }
}
