using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class UdpState
    {
        public UdpClient Client { get; set; }
        public IPEndPoint EndPoint { get; set; }
    }
}
