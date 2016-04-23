using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using IMClient.model;
using IMClient.model.events;
using IMClient.utils;
using IMLibrary;

namespace IMClient.controller
{
    public class ImClientHandler
    {
        private Thread _clientThread;
        private Client _clientInfo;
        private readonly string _ipAddress;
        private readonly int _portAddress;
        private List<IMLibrary.Client> _unbindedClientList;
        private Connection _connectionToServer;
        private bool _registerMode;
        private ObservableCollection<IMLibrary.Client> _generatedClientList;
        public RSAParameters PublicKey;

        private List<string> _messagesRecieved = new List<string>();


        public List<string> MessagesRecieved
        {
            get { return _messagesRecieved; }
        }
        public Connection ConnectionToServer
        {
            get { return _connectionToServer; }
        }

        public ObservableCollection<IMLibrary.Client> GeneratedClientList
        {
            get { return _generatedClientList; }

            set { _generatedClientList = value; }
        }

        public Client ClientInfo
        {
            get { return _clientInfo; }

            set { _clientInfo = value; }
        }

        public Thread ClientThread
        {
            get { return _clientThread; }

            set { _clientThread = value; }
        }

        void Connect(Client client, bool registering)
        {
            this._registerMode = registering;
            this._clientInfo = client;
            this._clientThread = new Thread(HandleClientSetup);
            _clientThread.Start();
        }

        public void Login(Client client)
        {
            Connect(client, false);
        }

        public void Register(Client client)
        {
            Connect(client, true);
        }

        public ImClientHandler(string ipAddress, int portAddress)
        {
            this._ipAddress = ipAddress;
            this._portAddress = portAddress;
        }
        void HandleClientSetup()
        {
            try
            {
                this._connectionToServer = new Connection(new TcpClient(this._ipAddress, this._portAddress));
                AuthenticateWithServer();
                byte autenticationResponse = ConnectionToServer.BinaryReader.ReadByte();
                switch (autenticationResponse)
                {
                    case IMCommands.IM_READY:
                        this._clientInfo.LoggedIn = true;
                        //new set of keys are generated every time a user logs in
                        Cryptography.GenerateKeys(out PublicKey);
                        //send the server the public key, allowing other clients to send you messages you can decrypt.
                        TransmitPublicKey();
                        GetUpdatedList();
                        OnLoginAuthorised();
                        HandleReceive();
                        break;
                    case IMCommands.ERROR_IM_USER_DOES_NOT_EXIST:
                        OnServerError(new ImServerErrorArgs("User does not exist."));
                        CloseConnection();
                        break;
                    case IMCommands.ERROR_IM_PASSWRONG:
                        OnServerError(new ImServerErrorArgs("Password incorrect."));
                        CloseConnection();
                        break;
                    case IMCommands.ERROR_IM_REGISTRATION_DEATILS_WRONG:
                        OnServerError(new ImServerErrorArgs("Incorrect details provided."));
                        CloseConnection();
                        break;
                    case IMCommands.ERROR_IM_USER_EXSISTS:
                        OnServerError(new ImServerErrorArgs("This username has already been taken."));
                        CloseConnection();
                        break;
                    case IMCommands.ERROR_USER_ALREADY_LOGGED_IN:
                        OnServerError(new ImServerErrorArgs("This user is already logged in."));
                        CloseConnection();
                        break;
                    case IMCommands.ERROR_UNKNOWN_COMMAND:
                        OnServerError(new ImServerErrorArgs("Unknown command."));
                        CloseConnection();
                        break;

                }
            }
            catch (Exception e)
            {
                    CloseConnection();
            }
        }

        void HandleReceive()
        {
            ImReceivedMessageEventArgs args = null;
            while (ConnectionToServer.TcpClient.Connected)
            {
                byte responseFromServer = ConnectionToServer.BinaryReader.ReadByte();
                switch (responseFromServer)
                {
                    case IMCommands.Buddies:
                        byte action = ConnectionToServer.BinaryReader.ReadByte();
 
                        switch (action)
                        {
                            case IMCommands.AddBuddy:
                                string usernameToAdd = ConnectionToServer.BinaryReader.ReadString();
                                if (!_generatedClientList.ToList().Single(cli => cli.UserName == _clientInfo.UserName).Buddies.Contains(usernameToAdd))
                                    _generatedClientList.ToList().Single(cli => cli.UserName == _clientInfo.UserName).Buddies.Add(usernameToAdd);
                                break;
                            case IMCommands.RemoveBuddy:
                                string usernameToRemove = ConnectionToServer.BinaryReader.ReadString();
                                if (_generatedClientList.ToList().Single(cli => cli.UserName == _clientInfo.UserName).Buddies.Contains(usernameToRemove))
                                    _generatedClientList.ToList().Single(cli => cli.UserName == _clientInfo.UserName).Buddies.Remove(usernameToRemove);
                                break;
                        }
                        OnBuddyUpdate(new EventArgs());
                        break;
                    case IMCommands.IM_RECEIVE:
                        try
                        {
                            string from = ConnectionToServer.BinaryReader.ReadString();
                            string msg = ConnectionToServer.BinaryReader.ReadString();
                            int encryptionStandardFlag = ConnectionToServer.BinaryReader.ReadInt32();
                            int encryptionMethodFlag = ConnectionToServer.BinaryReader.ReadInt32();
                            bool syncFlag = ConnectionToServer.BinaryReader.ReadBoolean();
                            args = new ImReceivedMessageEventArgs(new Message(msg, "", from, encryptionStandardFlag, encryptionMethodFlag, syncFlag));
                            _messagesRecieved.Add(msg);
                            if (GeneratedClientList.Count(cli => cli.UserName == from) == 0)
                            {

                                ConnectionToServer.BinaryWriter.Write(IMCommands.IM_CLIENT_LIST); // ask for list, although if we've made an attempt before they dont need to know what we're after
                            }
                            else
                            {
                                OnChatPromtCheck(args);
                                args = null; //nullify them
                            }
                        }
                        catch (Exception e)
                        {
                            e.GetType();
                        }
                        break;
                    case IMCommands.IM_CLIENT_LIST:
                        try
                        {
                            int sizeOfListBeingSent = ConnectionToServer.BinaryReader.ReadInt32(); //read list count
                            int lengthOfBytes = ConnectionToServer.BinaryReader.ReadInt32(); //read length of bytes incomming

                            byte[] incommingList = ConnectionToServer.SslStream.ReadStreamTillEnd(lengthOfBytes); //read them into byte array

                            _unbindedClientList = (List<IMLibrary.Client>) incommingList.ByteStreamToObject(typeof (List<IMLibrary.Client>));

                            //this event populates the the list view with the new clients
                            OnNewList(new ImListReceivedFromServerArgs(_unbindedClientList));
                            //ConnectionToServer.BinaryWriter.Write(sizeOfListBeingSent == unbindedClientList.Count ? IMCommands.IM_CORRECT : IMCommands.ERROR_INCORRECT);
                            //thise event invokes new negotiations with the new clients, serperate events so no network streams are crossed.
                            OnNegotiation(new InitiateNegotiationEventArgs(_unbindedClientList));
                            if (args != null)
                            {
                                OnChatPromtCheck(args); //if when we are receiving, if the client sender wasnt on the list we get a new list an then use the args
                                args = null; //nullify args
                            }
                        }
                        catch (Exception e)
                        {
                            e.GetType();
                        }
                        break;
                    case IMCommands.NegotiateKeys:
                        try
                        {
                            int tupleLength = ConnectionToServer.BinaryReader.ReadInt32();
                            byte[] tupleBytes = ConnectionToServer.BinaryReader.ReadBytes(tupleLength);
                            Tuple<byte[], byte[]> encrypttedKeyVectorCombo = (Tuple<byte[], byte[]>) tupleBytes.ByteStreamToObject(typeof (Tuple<byte[], byte[]>));
                            string sender = ConnectionToServer.BinaryReader.ReadString();
                            OnKeyNegotiation(new KeyNegotiationEventArgs(encrypttedKeyVectorCombo, sender));
                            ConnectionToServer.BinaryWriter.Write(IMCommands.IM_CLIENT_LIST); // ask for list, although if we've made an attempt before they dont need to know what we're after
                        } catch (Exception e)
                        {
                            e.GetType();
                        }

                        break;
                }
            }
        }

        public void SendMessage(Message msg)
        {


            if (!this._clientInfo.LoggedIn)
                return;

            byte[] messageBytes = msg.ObjectToByte();
            if (messageBytes.Length > 16000)
            {
                OnServerError(new ImServerErrorArgs("Message too long."));
                return;
            }


            this.ConnectionToServer.BinaryWriter.Write(IMCommands.IM_SEND);
            this.ConnectionToServer.BinaryWriter.Write(messageBytes.Length);
            this.ConnectionToServer.SslStream.Write(messageBytes);
            this.ConnectionToServer.SslStream.Write(IMCommands.Finished);

        }

        public void Buddy(string username, bool remove)
        {
            try
            {
                if (!this._clientInfo.LoggedIn)
                    return;

                this.ConnectionToServer.BinaryWriter.Write(IMCommands.Buddies);

                if (remove)
                {
                    this.ConnectionToServer.BinaryWriter.Write(IMCommands.RemoveBuddy);
                    this.ConnectionToServer.BinaryWriter.Write(username);
                }
                else
                {
                    this.ConnectionToServer.BinaryWriter.Write(IMCommands.AddBuddy);
                    this.ConnectionToServer.BinaryWriter.Write(username);
                }

            }
            catch
            {

            }
        }




        public void TransmitPublicKey()
        {
            try
            {
                if (!this._clientInfo.LoggedIn)
                    return;

                this.ConnectionToServer.BinaryWriter.Write(IMCommands.PublicKey);
                byte[] publicKeyBytes = PublicKey.ObjectToByte();
                this.ConnectionToServer.BinaryWriter.Write(publicKeyBytes.Length);
                this.ConnectionToServer.BinaryWriter.Write(publicKeyBytes);

            }
            catch (Exception e)
            {
                e.GetType();
            }
        }

        public void NegotiateSymmetricKeys(Tuple<byte[], byte[]> keyIvCombo, string clientName)
        {
            try
            {
                if (!this._clientInfo.LoggedIn)
                    return;

                this.ConnectionToServer.BinaryWriter.Write(IMCommands.NegotiateKeys);
                byte[] keyVectoryTupleAsBytes = keyIvCombo.ObjectToByte();
                this.ConnectionToServer.BinaryWriter.Write(keyVectoryTupleAsBytes.Length);
                this.ConnectionToServer.BinaryWriter.Write(keyVectoryTupleAsBytes);

                this.ConnectionToServer.BinaryWriter.Write(clientName);


            }
            catch (Exception e)
            {
                e.GetType();
            }
        }



        public void test()
        {
            foreach (IMLibrary.Client client in GeneratedClientList)
            {
                client.Available = false;
            }
        }

        public void GetUpdatedList()
        {
            try
            {
                SendRequestForList();

                //if (_unbindedClientList == null)
                //    return;
                // we do this method once, this allows us to control the second list call 
                this._unbindedClientList = new List<IMLibrary.Client>();
                byte response = ConnectionToServer.BinaryReader.ReadByte();
                if (response == IMCommands.IM_CLIENT_LIST)
                {
                    int sizeOfListBeingSent = ConnectionToServer.BinaryReader.ReadInt32(); //read list count
                    int lengthOfBytes = ConnectionToServer.BinaryReader.ReadInt32(); //read length of bytes incomming

                    byte[] incommingList = ConnectionToServer.SslStream.ReadStreamTillEnd(lengthOfBytes);
                    _unbindedClientList = (List<IMLibrary.Client>) incommingList.ByteStreamToObject(typeof (List<IMLibrary.Client>));

                    GeneratedClientList = new ObservableCollection<IMLibrary.Client>(_unbindedClientList);
                }

            }
            catch (Exception e)
            {
                e.GetType();
            }
        }

        public void SendRequestForList()
        {
            try
            {
                ConnectionToServer.BinaryWriter.Write(IMCommands.IM_CLIENT_LIST); // ask for list, although if we've made an attempt before they dont need to know what we're after
                ConnectionToServer.BinaryWriter.Flush();
            }
            catch
            {
                
            }
        }

        public bool IsServerAvailable()
        {
            try {
                var tcpClient = new TcpClient();
                var connectionResult = tcpClient.BeginConnect(this._ipAddress, this._portAddress, null, null);
                var connectionSuccessful = connectionResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

                if (connectionSuccessful)
                {
                    this._connectionToServer = new Connection(tcpClient);
                    if (_connectionToServer == null) throw new Exception("Failed to connect to server");
                    using (ConnectionToServer.NetworkStream)
                    { //using cleans up any mess
                        using (ConnectionToServer.BinaryReader)
                        {
                            using (ConnectionToServer.BinaryWriter)
                            {
                                ConnectionToServer.BinaryWriter.Write(IMCommands.IM_PING); //ping server to let them know we know we the server is available and let them disconnect us.
                                ConnectionToServer.BinaryWriter.Flush();
                                CloseConnection();
                            }
                        }
                    }
                    return true;
                }
            }
            catch (Exception e) {

            }
            throw new Exception("Server not available");
        }
        
        

        void AuthenticateWithServer()
        {
            try
            {
                ConnectionToServer.BinaryWriter.Write(IMCommands.IM_SETUP);
                ConnectionToServer.BinaryWriter.Write(this._registerMode ? IMCommands.IM_REGISTER : IMCommands.IM_LOGIN);
                ConnectionToServer.BinaryWriter.Flush();

                int receiveAuth = ConnectionToServer.BinaryReader.ReadInt32();

                if (receiveAuth != IMCommands.IM_AUTHENTICATE) return;

                ConnectionToServer.BinaryWriter.Write(IMCommands.IM_AUTHENTICATE);
                ConnectionToServer.BinaryWriter.Flush();
                ConnectionToServer.BinaryWriter.Write(_clientInfo.UserName);
                ConnectionToServer.BinaryWriter.Write(Cryptography.Sha1(_clientInfo.UserPassword));
                ConnectionToServer.BinaryWriter.Flush();
            } catch (Exception e)
            {
                e.GetType();
            }
        }


        void CloseConnection()
        {
            if (_clientInfo != null && _clientInfo.LoggedIn)
                this._clientInfo.LoggedIn = false;
            
            ConnectionToServer.CloseConnection();
        }
        
        //events
        public event EventHandler LoginAuthorised;
        public event IMChatPromtEventHandler ChatPromtCheck;
        public event ImServerErrorEventHandler ServerError;
        public event ImListEventHandler ListEventHandler;
        public event KeyNegotiationEventHandler KeyNegotiationEventHandler;
        public event NegotiationEventHandler NegotiationEventHandler;
        public event EventHandler BuddyEventHandler;

        virtual protected void OnBuddyUpdate(EventArgs e)
        {
            if (BuddyEventHandler != null)
                BuddyEventHandler(this, e);
        }
        virtual protected void OnNegotiation(InitiateNegotiationEventArgs e)
        {
            if (NegotiationEventHandler != null)
                NegotiationEventHandler(this, e);
        }
        virtual protected void OnKeyNegotiation(KeyNegotiationEventArgs e)
        {
            if (KeyNegotiationEventHandler != null)
                KeyNegotiationEventHandler(this, e);
        }
        virtual protected void OnNewList(ImListReceivedFromServerArgs e)
        {
            if (ListEventHandler != null)
                ListEventHandler(this, e);
        }

        virtual protected void OnChatPromtCheck(ImReceivedMessageEventArgs e)
        {
            if (ChatPromtCheck != null)
                ChatPromtCheck(this, e);
        }
        virtual protected void OnLoginAuthorised()
        {
            if (LoginAuthorised != null)
                LoginAuthorised(this, EventArgs.Empty);
        }
        virtual protected void OnServerError(ImServerErrorArgs e)
        {
            if (ServerError != null)
                ServerError(this, e);
        }
    }
}

