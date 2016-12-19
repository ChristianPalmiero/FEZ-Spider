using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerPack
{
    public static class Program
    {
        public static int Main(String[] args)
        {
            Server server = new Server();
            server.StartListening();
            return 0;
        }
    }
}
