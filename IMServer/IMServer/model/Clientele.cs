using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using IMLibrary;
namespace IMServer
{
    public class Clientele
    {
        public Dictionary<string, ClientInformation> _imUsers;
        private string fileName;

        public void SaveDictionaryToFile()
        {
            try
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                using (FileStream fileStream = new FileStream(this.fileName, FileMode.Create, FileAccess.Write))
                {
                    binaryFormatter.Serialize(fileStream, _imUsers.Values.ToArray());
                }
                ConsoleBuffer.PrintLine("Dictionary saved as list to file.");
            }
            catch (Exception e)
            {
                ConsoleBuffer.PrintLine("Exeception Occured, could not write to file." + e.ToString());
            }
        }

        public void LoadDictionaryFromFile()
        {
            try
            {
                using (FileStream fileStream = new FileStream(this.fileName, FileMode.Open, FileAccess.Read))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    ClientInformation[] clientInfoAsArray = (ClientInformation[])binaryFormatter.Deserialize(fileStream);

                    this._imUsers = clientInfoAsArray.ToDictionary(client => client.Username, client => client);
                }
                ConsoleBuffer.PrintLine("File loaded as dictionary." + "Dictionary count: " + this._imUsers.Count + ".");
            }
            catch (Exception e)
            {
                ConsoleBuffer.PrintLine("Exeception Occured, could not load file." + e.ToString());
            }
        }


        public void ClearDictionary()
        {
            this._imUsers.Clear();
            SaveDictionaryToFile();
        }
        public Clientele(string fileName)
        {
            _imUsers = new Dictionary<string, ClientInformation>();
            this.fileName = fileName;
            this.LoadDictionaryFromFile();
           
            
        }

        public bool UserExists(string key) { return this._imUsers.ContainsKey(key); }

        public string GetSalt(string key)
        {
            if (this._imUsers.Count > 0)
                return this._imUsers[key].Salt;

            return "";
        }

        public int ConnectedUserCount()
        {
            if (_imUsers.Count > 0)
                return _imUsers.Count(kvp => kvp.Value.LoggedIn == true);

            return 0;
        }

        public List<Client> ConnectedUsersAsList()
        {
            Client client;
            SaveDictionaryToFile();
            List<Client> clientList = new List<Client>(); //we send this to the client, we make sure to only send required details.
            foreach (KeyValuePair<string,ClientInformation> pair in this._imUsers)
            {
                client = new Client(pair.Key, pair.Value.LoggedIn, pair.Value.FingerPrint, pair.Value.PublicKey, pair.Value.Buddies) ;
                // ConsoleBuffer.PrintLine(pair.Key + pair.Value);
                clientList.Add(client);
            }

            return clientList;
        }
    }
}
