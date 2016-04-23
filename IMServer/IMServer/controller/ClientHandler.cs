using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using IMLibrary;
using IMServer.utils;

namespace IMServer
{
    public class ClientHandler
    {
        private readonly Clientele _clientele;
        private readonly TcpClient _tcpClient;
        private readonly Thread serverThread;
        private readonly X509Certificate2 _sslCertificate = new X509Certificate2(Path.GetFullPath(@""), @"");
        private BinaryReader _binaryReader;
        private BinaryWriter _binaryWriter;
        private ClientInformation _clientInfo; //client info
        private NetworkStream _networkStream;
        private SslStream _sslStream;

        public ClientHandler(Clientele clientele, TcpClient tcpClient)
        {
            _clientele = clientele;
            _tcpClient = tcpClient;
            serverThread = new Thread(HandleClientRecevier);
            serverThread.Start();
        }

        private void HandleClientSetup(byte command) // setting up connection, handling login and regsiter.
        {
            ConsoleBuffer.PrintLine("Handing client setup...");
            _binaryWriter.Write(IMCommands.IM_AUTHENTICATE);
            _binaryWriter.Flush();
            var authenticateResponse = _binaryReader.ReadInt32();
            if (authenticateResponse == IMCommands.IM_AUTHENTICATE)
            {
                var userName = _binaryReader.ReadString();
                var userPass = _binaryReader.ReadString();
                ConsoleBuffer.PrintLine("Recieved client details...");
                switch (command)
                {
                    case IMCommands.IM_LOGIN:
                        if (_clientele._imUsers.TryGetValue(userName, out _clientInfo))
                        {
                            if (_clientInfo.LoggedIn != true)
                            {
                                if (Cryptography.CompareValues(userPass, _clientInfo.Password))
                                {
                                    _clientInfo.LoggedIn = true;
                                    _clientInfo.ClientConnection = this;
                                    _binaryWriter.Write(IMCommands.IM_READY); //let client know that server is ready to communicate
                                    _binaryWriter.Flush();
                                    ConsoleBuffer.PrintLine("The following user has logged in.");
                                    ConsoleBuffer.PrintUserDetails(_clientInfo);
                                }
                                else
                                {
                                    ConsoleBuffer.PrintLine("Incorrect password entered for user " + _clientInfo.Username + ".");
                                    _binaryWriter.Write(IMCommands.ERROR_IM_PASSWRONG);
                                    _binaryWriter.Flush();
                                    CloseConnection();
                                }
                            }
                            else
                            {
                                _clientInfo = null;
                                _binaryWriter.Write(IMCommands.ERROR_USER_ALREADY_LOGGED_IN);
                                _binaryWriter.Flush();
                                CloseConnection();
                            }
                        }
                        else
                        {
                            ConsoleBuffer.PrintLine("Error; user does" + " '" + userName + "' " + "not exist" + ".");
                            _binaryWriter.Write(IMCommands.ERROR_IM_USER_DOES_NOT_EXIST);
                            _binaryWriter.Flush();
                            CloseConnection();
                        }
                        break;
                    case IMCommands.IM_REGISTER:
                        if (userName != "" || userPass != "")
                        {
                            if (!_clientele.UserExists(userName))
                            {
                                _clientInfo = new ClientInformation(userName, userPass);
                                _clientInfo.Buddies = new List<string>();
                                _clientele._imUsers.Add(userName, _clientInfo);
                                _clientele.SaveDictionaryToFile();
                                _binaryWriter.Write(IMCommands.IM_READY);
                                _binaryWriter.Flush();
                                _clientInfo.LoggedIn = true;
                                _clientInfo.ClientConnection = this;
                                ConsoleBuffer.PrintLine("The following user has registered and logged in.");
                                ConsoleBuffer.PrintUserDetails(_clientInfo);
                            }
                            else
                            {
                                _binaryWriter.Write(IMCommands.ERROR_IM_USER_EXSISTS);
                                _binaryWriter.Flush();
                                CloseConnection();
                            }
                        }
                        else
                        {
                            _binaryWriter.Write(IMCommands.ERROR_IM_REGISTRATION_DEATILS_WRONG);
                            _binaryWriter.Flush();
                            CloseConnection();
                        }
                        break;
                    case IMCommands.IM_LOGOUT:
                        if (_clientInfo.LoggedIn)
                        {
                            _clientInfo.ClientConnection.CloseConnection();
                        }
                        else
                        {
                            _binaryWriter.Write(IMCommands.ERROR_IM_USER_NOT_LOGGED_IN);
                            _binaryWriter.Flush();
                            CloseConnection();
                        }
                        break;
                    default:
                        CloseConnection();
                        _binaryWriter.Write(IMCommands.ERROR_UNKNOWN_COMMAND);
                        break;
                }
            }
            else
            {
                CloseConnection();
            }
        }

        private void CloseConnection()
        {
            try
            {
                if (_tcpClient.Connected)
                {
                    if (_clientInfo != null) //connection is being closed by an actual client
                    {
                        ConsoleBuffer.PrintLine(_clientInfo.Username + " is trying to close connection.");
                        _tcpClient.Close();
                        _clientInfo.LoggedIn = false;
                        _clientInfo.ClientConnection = null;
                    }
                    else
                    {
                        _tcpClient.Close(); //connection closed by client that is not logged in, example a client ping.
                    }
                }
                else
                {
                    _tcpClient.Close();
                    _clientInfo.LoggedIn = false;
                    _clientInfo.ClientConnection = null;
                    ConsoleBuffer.PrintLine("Client force closed the connection.");
                }
            }
            catch
            {
                _tcpClient.Close();
            }
            ConsoleBuffer.PrintLine("Connectioned terminated.");
        }

        private void HandleClientRecevier()
        {
            try
            {
                var ep = _tcpClient.Client.RemoteEndPoint as IPEndPoint;
                ConsoleBuffer.PrintLine("Connection started, sender address. = " + ep.Address);

                using (_networkStream = _tcpClient.GetStream())
                {
                    ConsoleBuffer.PrintLine("Getting network stream...");
                    using (_sslStream = new SslStream(_networkStream))
                    {
                        ConsoleBuffer.PrintLine("Getting ssl stream...");
                        _sslStream.AuthenticateAsServer(_sslCertificate, false, SslProtocols.Tls, true);
                        ConsoleBuffer.PrintLine("Authenticated ssl stream...");
                        using (_binaryReader = new BinaryReader(_sslStream, Encoding.UTF8))
                        {
                            using (_binaryWriter = new BinaryWriter(_sslStream, Encoding.UTF8))
                            {
                                while (_tcpClient.Client.Connected)
                                {
                                    ConsoleBuffer.PrintLine("Waiting for request...");
                                    var command = _binaryReader.ReadByte();
                                    ConsoleBuffer.PrintLine("Recieved request = " + command);
                                    switch (command)
                                    {
                                        case IMCommands.Buddies:
                                            var action = _binaryReader.ReadByte();
                                            switch (action)
                                            {
                                                case IMCommands.AddBuddy:
                                                    var usernameToAdd = _binaryReader.ReadString();

                                                    if (!_clientInfo.Buddies.ToList().Contains(usernameToAdd))
                                                    {
                                                        _clientInfo.Buddies.Add(usernameToAdd);
                                                        _binaryWriter.Write(IMCommands.Buddies);
                                                        _binaryWriter.Write(IMCommands.AddBuddy);
                                                        _binaryWriter.Write(usernameToAdd);
                                                    }
                                                    break;
                                                case IMCommands.RemoveBuddy:
                                                    var usernameToRemove = _binaryReader.ReadString();

                                                    if (_clientInfo.Buddies.ToList().Contains(usernameToRemove))
                                                    {
                                                        _clientInfo.Buddies.Remove(usernameToRemove);
                                                        _binaryWriter.Write(IMCommands.Buddies);
                                                        _binaryWriter.Write(IMCommands.RemoveBuddy);
                                                        _binaryWriter.Write(usernameToRemove);
                                                    }
                                                    break;
                                            }
                                            _clientele.SaveDictionaryToFile();
                                            break;
                                        case IMCommands.NegotiateKeys:
                                            try
                                            {
                                                var tupleLength = _binaryReader.ReadInt32();
                                                var tupleBytes = _binaryReader.ReadBytes(tupleLength);

                                                var username = _binaryReader.ReadString();

                                                ClientInformation clientNegotiatingWith;
                                                if (_clientele._imUsers.TryGetValue(username, out clientNegotiatingWith))
                                                {
                                                    if (clientNegotiatingWith.LoggedIn)
                                                    {
                                                        clientNegotiatingWith.ClientConnection._binaryWriter.Write(IMCommands.NegotiateKeys);
                                                        clientNegotiatingWith.ClientConnection._binaryWriter.Write(tupleBytes.Length);
                                                        clientNegotiatingWith.ClientConnection._binaryWriter.Write(tupleBytes);
                                                        clientNegotiatingWith.ClientConnection._binaryWriter.Write(_clientInfo.Username);
                                                    }
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                ConsoleBuffer.PrintLine(e.StackTrace);
                                            }
                                            break;
                                        case IMCommands.Verify:
                                            break;
                                        case IMCommands.PublicKey:
                                            try
                                            {
                                                //retrieve the sent public key
                                                var byteFileArrayLength = _binaryReader.ReadInt32();
                                                var incommingPackets = _binaryReader.ReadBytes(byteFileArrayLength);
                                                var publicKey = (RSAParameters) incommingPackets.ByteStreamToObject(typeof (RSAParameters));
                                                _clientInfo.PublicKey = publicKey;
                                                //generate a finger print for the user.
                                                using (var stringWriter = new StringWriter())
                                                {
                                                    //serialise the rsa key into xml
                                                    var keySerialisedAsXml =
                                                        new XmlSerializer(typeof (RSAParameters));
                                                    keySerialisedAsXml.Serialize(stringWriter, publicKey);
                                                    //retreive the xml content as a string
                                                    var publicKeyAsXmlString = stringWriter.ToString();
                                                    //generate a Sha1 hash of the string
                                                    _clientInfo.FingerPrint = Cryptography.Sha1(publicKeyAsXmlString);
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                ConsoleBuffer.PrintLine(e.StackTrace);
                                            }
                                            break;
                                        case IMCommands.IM_SEND:
                                            try
                                            {
                                                var byteArrayLength = _binaryReader.ReadInt32();
                                                var incommingMessagePacket = _sslStream.ReadStreamTillEnd(byteArrayLength);
                                                var messagePacket = (Message) incommingMessagePacket.ByteStreamToObject(typeof (Message));
                                                ClientInformation recipient;

                                                if (_clientele._imUsers.TryGetValue(messagePacket.To, out recipient))
                                                {
                                                    if (recipient.LoggedIn)
                                                    {
                                                        recipient.ClientConnection._binaryWriter.Write(IMCommands.IM_RECEIVE);
                                                        recipient.ClientConnection._binaryWriter.Write(messagePacket.From);
                                                        recipient.ClientConnection._binaryWriter.Write(messagePacket.MessageSent);
                                                        recipient.ClientConnection._binaryWriter.Write(messagePacket.EncryptionStandardFlag);
                                                        recipient.ClientConnection._binaryWriter.Write(messagePacket.EncryptionMethodFlag);
                                                        recipient.ClientConnection._binaryWriter.Write(messagePacket.SyncFlag);

                                                        recipient.ClientConnection._binaryWriter.Flush();
                                                        ConsoleBuffer.PrintLine("[" + _clientInfo.Username + "]" + " " + "sent message to" + " [" + recipient.Username + "]");
                                                    }
                                                    else
                                                    {
                                                        ConsoleBuffer.PrintLine("[" + _clientInfo.Username + "]" + " " + "sent message to" + " [" + recipient.Username + "]");
                                                    }
                                                }
                                                else
                                                {
                                                    ConsoleBuffer.PrintLine("Error; unknown receipient " + recipient.Username);
                                                    _binaryWriter.Write(IMCommands.ERROR_UNKNOWN_RECIPIENT);
                                                    _binaryWriter.Flush();
                                                }
                                            }
                                            catch
                                            {
                                                _networkStream.Flush();
                                            }
                                            break;
                                        case IMCommands.IM_SETUP:

                                            var setupCommand = _binaryReader.ReadByte();
                                            HandleClientSetup(setupCommand);
                                            break;
                                        case IMCommands.IM_CLIENT_LIST:
                                            try
                                            {
                                                _binaryWriter.Write(IMCommands.IM_CLIENT_LIST);
                                                _binaryWriter.Write(_clientele._imUsers.Count);
                                                byte[] bytes;
                                                bytes = _clientele.ConnectedUsersAsList().ObjectToByte();
                                                _binaryWriter.Write(bytes.Length);
                                                _sslStream.Write(bytes);
                                                _sslStream.Write(IMCommands.Finished);
                                                //byte finalResponse = _binaryReader.ReadByte();
                                                //if (finalResponse != IMCommands.IM_CORRECT)
                                                //{
                                                //    _binaryWriter.Write(IMCommands.IM_RETRY);
                                                //    ConsoleBuffer.PrintLine("\tFailed to send list.");
                                                //}
                                            }
                                            catch (Exception e)
                                            {
                                                ConsoleBuffer.PrintLine(e.StackTrace);
                                            }

                                            break;
                                        case IMCommands.IM_PING:
                                            ConsoleBuffer.PrintLine("Server was pinged by: " + ep.Address);
                                            CloseConnection(); //user is pinging to check if the connection is available
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CloseConnection();
            }
        }
    }
}