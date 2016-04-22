using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Security;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using IMLibrary;
using System.Diagnostics;

namespace IMServer
{
    public class ServerListener : TcpListener
    {
        private Clientele _currentClients;
        private IPAddress _ipAddress;
        private int _portAddress;
        public ServerListener(IPAddress ipAddress, int portAddress) : base(ipAddress, portAddress)
        {
            this.IpAddress = ipAddress;
            this.PortAddress = portAddress;
            this._currentClients = new Clientele(Environment.CurrentDirectory + "\\userinfo.dat");

            ConsoleBuffer.PrintLine("Server started on socket: " + ipAddress + ":" + portAddress);
        }

        public Clientele CurrentClients
        {
            get { return _currentClients; }
        }

        public IPAddress IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }
        }

        public int PortAddress
        {
            get { return _portAddress; }
            set { _portAddress = value; }
        }

        public bool ServerActive
        {
            get { return this.Active; }
        }

        public void StartListener()
        {
            TcpClient tcpClient;
            ClientHandler clientHandler;
            //_currentClients.ClearDictionary();
            //_currentClients.SaveDictionaryToFile();
            while (this.Active)
            {
                tcpClient = this.AcceptTcpClient();
                clientHandler = new ClientHandler(CurrentClients, tcpClient);
                Thread.Sleep(10); //avoid cpu locking
            }
        }

        public void StopServer()
        {
            this._currentClients._imUsers.ToList().ForEach(client => client.Value.LoggedIn = false); //all users are going to be logged out..
            this.Stop();
        }

        public void StartServer(object f)
        {
            try {
                this.Start();
                StartListener();
     
            } catch (SocketException s)
            {
                ConsoleBuffer.PrintLine(s.ToString());
                Console.Read();
            }
        }

    }
}
