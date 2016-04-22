using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.Security;
using System.Net;
using System.Threading;


namespace IMServer
{
    public class Server
    {
        private readonly ServerListener _imServer;

        public Server()
        {
            ConsoleBuffer.PrintLine("--------------Instant Messenger Server--------------");
            ConsoleBuffer.PrintLine("Starting server...");
            this._imServer = new ServerListener(ServerInfo.GetIPAddress(), ServerInfo.ServerPortAddress());
        }

        public ServerListener ImServer
        {
            get
            {
                return _imServer;
            }

        }

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Server program = new Server();
            program.ImServer.StartServer(null);       
        }
    }

}
