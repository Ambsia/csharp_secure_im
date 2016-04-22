using Microsoft.VisualStudio.TestTools.UnitTesting;
using IMServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using IMLibrary;
using IMClient;
using IMClient.controller;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

namespace IMServer.Tests
{
    [TestClass()]
    public class ServerTests
    {
        private IMClient.Client[] _clients = new IMClient.Client[]
        {
            new IMClient.Client("Tim", "thunder"),
            new IMClient.Client("Bob", "abc"),
            new IMClient.Client("Alice", "qwerty"),
            new IMClient.Client("Mark", "great"),
            new IMClient.Client("Felix", "password"),
            new IMClient.Client("John", "cat"),
        };


        [TestMethod()]
        public void Server_Start_Test()
        {
            Server s = new Server();
            ThreadPool.QueueUserWorkItem(s.ImServer.StartServer, null);
            Thread.Sleep(20);
            Assert.IsTrue(s.ImServer.ServerActive);
            s.ImServer.StopServer();
        }

        [TestMethod]
        public void Is_Server_Available_Test()
        {
            Server s = new Server();
            ThreadPool.QueueUserWorkItem(s.ImServer.StartServer);
            Thread.Sleep(20);

            if (s.ImServer.ServerActive)
            {
                ImClientHandler clientAuth = new ImClientHandler(
                    s.ImServer.IpAddress.ToString(),
                    s.ImServer.PortAddress);
                Assert.IsTrue(clientAuth.IsServerAvailable());
            }
            s.ImServer.StopServer();
        }

        [TestMethod]
        public void Multiple_Register_Attempts()
        {
            Server s = new Server();
            ThreadPool.QueueUserWorkItem(s.ImServer.StartServer, null);

            s.ImServer.CurrentClients._imUsers.Clear();
            s.ImServer.CurrentClients.SaveDictionaryToFile();
            Thread.Sleep(20);

            int reg = 0;
            while (s.ImServer.ServerActive && reg != 6)
            {
                var clientAuth1 = new ImClientHandler(
                    s.ImServer.IpAddress.ToString(),
                    s.ImServer.PortAddress);
                clientAuth1.Register(_clients[reg]);
                Thread.Sleep(400);
                reg++;
            }
            Assert.IsTrue(s.ImServer.CurrentClients._imUsers.Count == 6);
            s.ImServer.StopServer();
        }

        [TestMethod]
        public void Multiple_Login_Attempts()
        {
            Server s = new Server();
            ThreadPool.QueueUserWorkItem(s.ImServer.StartServer, null);
            Thread.Sleep(20);

            int reg = 0;
            while (s.ImServer.ServerActive && reg != 6)
            {
                var clientAuth1 = new ImClientHandler(
                    s.ImServer.IpAddress.ToString(),
                    s.ImServer.PortAddress);
                clientAuth1.Login(_clients[reg]);
                Thread.Sleep(200);
                reg++;
            }
            Assert.IsTrue(
                s.ImServer.CurrentClients._imUsers.ToList()
                .Select(cli => cli.Value.LoggedIn).Count() == 6);
            s.ImServer.StopServer();
        }

        [TestMethod]
        public void Message_Send_Receive_Attempts()
        {
            Server s = new Server();
            ThreadPool.QueueUserWorkItem(s.ImServer.StartServer, null);
            Thread.Sleep(20);
            int reg = 0;
            List<ImClientHandler> clientList = new List<ImClientHandler>();
            while (s.ImServer.ServerActive && reg != 2)
            {
                clientList.Add(new ImClientHandler(
                    s.ImServer.IpAddress.ToString(),
                    s.ImServer.PortAddress));
                clientList[reg].Login(_clients[reg]);
                Thread.Sleep(200);
                reg++;
            }

            clientList[0].SendMessage(new Message("Test Message", "Bob", "", 0,0, true));
            clientList[1].SendMessage(new Message("Test Message", "Tim", "", 0,0, true));

            Thread.Sleep(1000);

            bool messagesReceived = clientList.Select(
                cli => cli.MessagesRecieved.Count > 0).Count() == 2;

            Assert.IsTrue(messagesReceived);
            s.ImServer.StopServer();
        }


        [TestMethod]
        public void Asymmetrical_Key_Regeneration_Attempt()
        {
            Server s = new Server();
            ThreadPool.QueueUserWorkItem(s.ImServer.StartServer, null);
            Thread.Sleep(20);

            ImClientHandler client = 
                new ImClientHandler(s.ImServer.IpAddress.ToString(), s.ImServer.PortAddress);

            client.Login(_clients[0]);
            Thread.Sleep(200);

            RSAParameters oldKey = client.PublicKey;
            IMClient.utils.Cryptography.DeleteKeyFromContainer();
            IMClient.utils.Cryptography.GenerateKeys(out client.PublicKey);

            Thread.Sleep(1000);

            Assert.IsFalse(oldKey.Equals(client.PublicKey));
            s.ImServer.StopServer();
        }


        [TestMethod]
        public void AES_Encryption_Decryption_Attempt_PASS()
        {
            const string message = "Test Message";
            byte[] key = { 23, 25, 26, 75, 76, 45, 34, 243 };
            const string salt = "=4JKF[4]!43";

            string encryptedMessage = 
                IMClient.utils.Cryptography.Encrypt<AesManaged>(message, key, salt);

            string decryptedMessage = 
                IMClient.utils.Cryptography.Decrypt<AesManaged>(encryptedMessage, key, salt);

            Assert.IsTrue(message == decryptedMessage);
        }

        [TestMethod]
        public void AES_Encryption_Decryption_Attempt_FAIL()
        {
            const string message = "Test Message";
            byte[] correctKey = { 23, 25, 26, 75, 76, 45, 34, 243 };
            const string correctSalt = "=4JKF[4]!43";

            string encryptedMessage = 
                IMClient.utils.Cryptography.Encrypt<AesManaged>(message, correctKey, correctSalt);

            byte[] incorrectKey = { 98, 25, 26, 255, 76, 21, 3, 24 };
            const string incorrectSalt = "8=OIF[4}(13";

            try
            {
                IMClient.utils.Cryptography
                    .Decrypt<AesManaged>(encryptedMessage, incorrectKey, incorrectSalt);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is CryptographicException);
            }
        }

        [TestMethod]
        public void TDES_Encryption_Decryption_Attempt_PASS()
        {
            const string message = "Test Message";

            byte[] key = { 23, 25, 26, 75, 76, 45, 34, 243 };
            const string salt = "=4JKF[4]!43";

            string encryptedMessage = 
                IMClient.utils.Cryptography.Encrypt<TripleDESCryptoServiceProvider>(
                    message, key, salt);
            string decryptedMessage = 
                IMClient.utils.Cryptography.Decrypt<TripleDESCryptoServiceProvider>(
                    encryptedMessage, key, salt);

            Assert.IsTrue(message == decryptedMessage);
        }

        [TestMethod]
        public void TDES_Encryption_Decryption_Attempt_FAIL()
        {
            const string message = "Test Message";

            byte[] correctKey = { 23, 25, 26, 75, 76, 45, 34, 243 };
            const string correctSalt = "=4JKF[4]!43";

            string encryptedMessage = 
                IMClient.utils.Cryptography.Encrypt<TripleDESCryptoServiceProvider>(
                    message, correctKey, correctSalt);

            byte[] incorrectKey = { 98, 25, 26, 255, 76, 21, 3, 24 };
            const string incorrectSalt = "8=OIF[4}(13";

            try
            {
                string decryptedMessage = 
                    IMClient.utils.Cryptography.Decrypt<TripleDESCryptoServiceProvider>(
                        encryptedMessage, incorrectKey, incorrectSalt);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is CryptographicException);
            }
        }

        //[TestMethod]
        //public void RC2_Encryption_Decryption_Attempt_PASS()
        //{
        //    const string message = "Test Message";
        //    byte[] key = new byte[] { 23, 25, 26, 75, 76, 45, 34, 243 };
        //    const string salt = "=4JKF[4]!43";

        //    string encryptedMessage = IMClient.utils.Cryptography.Encrypt<RC2CryptoServiceProvider>(message, key, salt);

        //    string decryptedMessage = IMClient.utils.Cryptography.Decrypt<RC2CryptoServiceProvider>(encryptedMessage, key, salt);

        //    Assert.IsTrue(message == decryptedMessage);
        //}

        //[TestMethod]
        //public void RC2_Encryption_Decryption_Attempt_FAIL()
        //{
        //    const string message = "Test Message";
        //    byte[] correctKey = new byte[] { 23, 25, 26, 75, 76, 45, 34, 243 };
        //    const string correctSalt = "=4JKF[4]!43";

        //    string encryptedMessage = IMClient.utils.Cryptography.Encrypt<RC2CryptoServiceProvider>(message, correctKey, correctSalt);

        //    byte[] incorrectKey = new byte[] { 98, 25, 26, 255, 76, 21, 3, 24 };
        //    const string incorrectSalt = "8=OIF[4}(13";

        //    try
        //    {
        //        string decryptedMessage = IMClient.utils.Cryptography.Decrypt<RC2CryptoServiceProvider>(encryptedMessage, incorrectKey, incorrectSalt);
        //        Assert.Fail();
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.IsTrue(ex is CryptographicException);
        //    }

        //}

        [TestMethod]
        public void One_Time_Pad_Encryption_Decryption_Attempt_PASS()
        {
            const string message = "Test Message";
            const string pad = "Test One Time Pad";

            string encryptedMessage = 
                IMClient.utils.Cryptography.EncryptMessageWithPad(pad, message);

            string decryptedMessage = 
                IMClient.utils.Cryptography.DecryptMessageWithPad(pad, encryptedMessage);
        
            Assert.IsTrue(message == decryptedMessage);
        }

        [TestMethod]
        public void One_Time_Pad_Encryption_Decryption_Attempt_FAIL()
        {
            const string message = "Test Message";

            const string correctPad = "Correct Test One Time Pad";
            string encryptedMessage = 
                IMClient.utils.Cryptography.EncryptMessageWithPad(correctPad, message);

            const string incorrectPad = "Incorrect Test One Time Pad";
            string decryptedMessage = 
                IMClient.utils.Cryptography.DecryptMessageWithPad(incorrectPad, encryptedMessage);

            Assert.IsTrue(message != decryptedMessage);
        }

    }
}

