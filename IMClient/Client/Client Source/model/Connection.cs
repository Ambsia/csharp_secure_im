using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.model
{
    //encapsulated connection
    public class Connection
    {
        TcpClient _tcpClient;
        NetworkStream _networkStream;
        BinaryReader _binaryReader;
        BinaryWriter _binaryWriter;
        SslStream _sslStream;
        public Connection(TcpClient tcpClient)
        {
            this._tcpClient = tcpClient;
            this._networkStream = tcpClient.GetStream();
            _sslStream = new SslStream(_networkStream, false, new RemoteCertificateValidationCallback(ValidateCert));
            _sslStream.AuthenticateAsClient("IMServer");
            this._binaryReader = new BinaryReader(_sslStream, Encoding.UTF8);
            this._binaryWriter = new BinaryWriter(_sslStream, Encoding.UTF8);
        }

        public TcpClient TcpClient
        {
            get
            {
                return _tcpClient;
            }
        }

        public NetworkStream NetworkStream
        {
            get
            {
                return _networkStream;
            }
        }

        public BinaryReader BinaryReader
        {
            get
            {
                return _binaryReader;
            }
        }

        public SslStream SslStream
        {
            get
            {
                return _sslStream;
            }
        }


        public BinaryWriter BinaryWriter
        {
            get
            {
                return _binaryWriter;
            }

        }

        public void CloseConnection()
        {
            _tcpClient.Close();
            _networkStream.Close();
            _binaryReader.Close();
            _binaryWriter.Close();
        }

        private static bool ValidateCert(object sender, X509Certificate certificate,
              X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Allow untrusted certificates.
        }

    }
}
