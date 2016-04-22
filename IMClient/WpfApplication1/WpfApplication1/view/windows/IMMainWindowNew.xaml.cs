using IMClient.controller;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using IMClient.model;
using IMClient.model.events;
using IMClient.utils;
using IMClient.view;
using WpfApplication1;

namespace IMClient
{
    /// <summary>
    /// Interaction logic for IMMainWindowNew.xaml
    /// </summary>
    /// 
    public class ChatDialogueLocal : ChatDialogue { }

    public partial class ImMainWindowNew
    {


        private readonly ImClientHandler _clientHandler;
        private readonly List<Tuple<IMLibrary.Client, ChatDialogueLocal>> _clientPromtList = new List<Tuple<IMLibrary.Client, ChatDialogueLocal>>();
        //private List<Pair<IMLibrary.Client, string>> _clientMessagesReceivedTupleList = new List<Pair<IMLibrary.Client, string>>();

        public ObservableCollection<User> GeneratedClientList;


        private readonly ICollectionView _filteredView;


        private Dictionary<string, Tuple<byte[], byte[]>> _clientKeyIvDictionary = new Dictionary<string, Tuple<byte[], byte[]>>();

        public List<Tuple<IMLibrary.Client, ChatDialogueLocal>> ClientPromtList
        {
            get { return _clientPromtList; }
        }

        public ImMainWindowNew(ImClientHandler clientHandler)
        {
            InitializeComponent();
            _clientHandler = clientHandler;

            GeneratedClientList = new ObservableCollection<User>();
            foreach (IMLibrary.Client client in _clientHandler.GeneratedClientList)
            {
                GeneratedClientList.Add(new User(client.UserName, client.Available, client.FingerPrint, client.PublicKey, client.Buddies, 0));
            }

            //GeneratedClientList.CollectionChanged += GeneratedClientList_CollectionChanged;

            _clientHandler.ListEventHandler += _clientHandler_ListEventHandler;
            _clientHandler.KeyNegotiationEventHandler += _clientHandler_KeyNegotiationEventHandler;
            _clientHandler.NegotiationEventHandler += _clientHandler_InitiateNegotiation;
            _clientHandler.BuddyEventHandler += _clientHandler_BuddyEventHandler;
            _filteredView = new CollectionViewSource {Source = GeneratedClientList}.View;

            DataContext = new
            {
                user = _clientHandler.ClientInfo,
                clients = _filteredView,
                //messages = _clientMessagesReceivedTupleList

            };
            _filteredView.Refresh();
            IMChatPromtEventHandler chatPromtEventHandler = MessageReceiveHandler;
            clientHandler.ChatPromtCheck += chatPromtEventHandler;

            foreach (User client in GeneratedClientList)
            {
                ChatDialogueLocal chatDialogoue = new ChatDialogueLocal {lblUsername = {Content = client.UserName}, lblFingerPrint = {Content = client.FingerPrint}};
                chatDialogoue.ButtonRegenerateKeys.Click += ButtonRegenerateKeys_Click;
                _clientPromtList.Add(new Tuple<IMLibrary.Client, ChatDialogueLocal>(client, chatDialogoue));
                //_clientMessagesReceivedTupleList.Add(new Pair<IMLibrary.Client, string>(client, "0"));
                if (client.FingerPrint != null)
                {
                    chatDialogoue.ChangeColourOfPrint();
                    chatDialogoue.LoadPrint();

                    //new instant of aes, key and IV are created and access through properties. f
                    try
                    {

                        Tuple<byte[], byte[]> unencryptedKeySaltPair = GenerateUnencryptedKeySaltPair();
                        Tuple<byte[], byte[]> encryptedKeySaltPair = EncryptKeySaltPair(client.PublicKey, unencryptedKeySaltPair);

                        if (client.UserName != _clientHandler.ClientInfo.UserName)
                        {
                            _clientHandler.NegotiateSymmetricKeys(encryptedKeySaltPair, client.UserName);
                        }

                        this._clientKeyIvDictionary.Add(client.UserName, unencryptedKeySaltPair);
                    }
                    catch (Exception e)
                    {
                        //will fail if we don't know the clients public key
                    }
                }
            }
        }



        private void ButtonRegenerateKeys_Click(object sender, RoutedEventArgs e)
        {
            Cryptography.DeleteKeyFromContainer();
            Cryptography.GenerateKeys(out _clientHandler.PublicKey);

            _clientHandler.TransmitPublicKey();
            _clientHandler.SendRequestForList();


        }

        private void _clientHandler_BuddyEventHandler(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _filteredView.Refresh();
            }));
        }

        private void _clientHandler_KeyNegotiationEventHandler(object sender, KeyNegotiationEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                string encryptedKeyAsString = System.Text.Encoding.Unicode.GetString(e.KeyVectorCombo.Item1);
                string encryptedSaltAsString = System.Text.Encoding.Unicode.GetString(e.KeyVectorCombo.Item2);

                string plainTextKeyAsString = Cryptography.DecryptData(encryptedKeyAsString);
                string plainTextSaltAsString = Cryptography.DecryptData(encryptedSaltAsString);

                byte[] key = System.Text.Encoding.Unicode.GetBytes(plainTextKeyAsString);
                byte[] salt = System.Text.Encoding.Unicode.GetBytes(plainTextSaltAsString);

                try
                {
                    if (this._clientKeyIvDictionary.ContainsKey(e.SenderUsername))
                    {
                        this._clientKeyIvDictionary.Remove(e.SenderUsername);
                    }
                    this._clientKeyIvDictionary.Add(e.SenderUsername, new Tuple<byte[], byte[]>(key, salt));

                }
                catch
                {

                }
            }));
        }


        private void _clientHandler_ListEventHandler(object sender, ImListReceivedFromServerArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (IMLibrary.Client client in e.ClientList)
                {
                    if (GeneratedClientList.Count(cli => cli.UserName == client.UserName) == 0)
                    {
                        ChatDialogueLocal chatDialogoue = new ChatDialogueLocal {lblUsername = {Content = client.UserName}, lblFingerPrint = {Content = client.FingerPrint}};

                        chatDialogoue.ChangeColourOfPrint();
                        chatDialogoue.LoadPrint();

                        User user = new User(client.UserName, client.Available, client.FingerPrint, client.PublicKey, client.Buddies, 0);
                        _clientPromtList.Add(new Tuple<IMLibrary.Client, ChatDialogueLocal>(user, chatDialogoue));

                        GeneratedClientList.Add(user);
                    }
                    else
                    {
                        GeneratedClientList.First(cli => cli.UserName == client.UserName).Available = client.Available;
                        GeneratedClientList.First(cli => cli.UserName == client.UserName).PublicKey = client.PublicKey;
                        GeneratedClientList.First(cli => cli.UserName == client.UserName).FingerPrint = client.FingerPrint;
                        _clientPromtList.Single(cli => cli.Item1.UserName == client.UserName).Item2.lblFingerPrint.Content = client.FingerPrint;
                        _clientPromtList.Single(cli => cli.Item1.UserName == client.UserName).Item2.ChangeColourOfPrint();
                        _clientPromtList.Single(cli => cli.Item1.UserName == client.UserName).Item2.LoadPrint();
                    }
                }
            }));
        }

        public void _clientHandler_InitiateNegotiation(object sender, InitiateNegotiationEventArgs e)
        {
            try
            {
                foreach (IMLibrary.Client client in e.ClientList)
                {
                    if (GeneratedClientList.Count(cli => cli.UserName == client.UserName) == 0)
                    {
                        //generate a new key/salt pair with the correct public key and negotiate with the client
                        Tuple<byte[], byte[]> unencryptedKeySaltPair = GenerateUnencryptedKeySaltPair();
                        Tuple<byte[], byte[]> encryptedKeySalePair = EncryptKeySaltPair(client.PublicKey, unencryptedKeySaltPair);
                        _clientHandler.NegotiateSymmetricKeys(encryptedKeySalePair, client.UserName);

                        if (this._clientKeyIvDictionary.ContainsKey(client.UserName))
                        {
                            this._clientKeyIvDictionary.Remove(client.UserName);
                        }
                        this._clientKeyIvDictionary.Add(client.UserName, unencryptedKeySaltPair);
                    }
                }
            }
            catch
            {

            }
        }

        private void MessageReceiveHandler(object obj, ImReceivedMessageEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (this._clientPromtList.Find(cli => cli.Item1.UserName == e.Message.From) == null)
                {
                    //if we hit here, a client that is not on the list has sent us a messsage, we could resync with the list.
                }
                string decryptedMessage = "";
                if (!gridBuddies.Children.Contains(_clientPromtList.Find(cli => cli.Item1.UserName == e.Message.From).Item2))
                {
                    GeneratedClientList.ToList().Find(cli => cli.UserName == e.Message.From).NewMessages = true;
                    GeneratedClientList.ToList().Find(cli => cli.UserName == e.Message.From).Received++;
                }
                try
                {
                    if (_clientKeyIvDictionary.ContainsKey(e.Message.From))
                    {
                        int selectedEncryptionMethod = e.Message.SyncFlag ? e.Message.EncryptionMethodFlag : _clientPromtList.Find(cli => cli.Item1.UserName == e.Message.From).Item2.EncryptionMethod.SelectedIndex;
                        _clientPromtList.Find(cli => cli.Item1.UserName == e.Message.From).Item2.EncryptionMethod.SelectedIndex = selectedEncryptionMethod;

                        switch (selectedEncryptionMethod)
                        {
                            case 0: //symmetrical decryption

                                int selectedStandard = e.Message.SyncFlag ? e.Message.EncryptionStandardFlag : _clientPromtList.Find(cli => cli.Item1.UserName == e.Message.From).Item2.EncryptionStandard.SelectedIndex;
                                _clientPromtList.Find(cli => cli.Item1.UserName == e.Message.From).Item2.EncryptionStandard.SelectedIndex = selectedStandard;
                                switch (selectedStandard)
                                {
                                    case 0:
                                        decryptedMessage = Cryptography.Decrypt<AesManaged>(e.Message.MessageSent, _clientKeyIvDictionary[e.Message.From].Item1, Encoding.Unicode.GetString(_clientKeyIvDictionary[e.Message.From].Item2));
                                        break;
                                    case 1:
                                        decryptedMessage = Cryptography.Decrypt<TripleDESCryptoServiceProvider>(e.Message.MessageSent, _clientKeyIvDictionary[e.Message.From].Item1, Encoding.Unicode.GetString(_clientKeyIvDictionary[e.Message.From].Item2));
                                        break;
                                    //case 2:
                                    //    decryptedMessage = Cryptography.Decrypt<RC2CryptoServiceProvider>(e.Message.MessageSent, _clientKeyIvDictionary[e.Message.From].Item1, Encoding.Unicode.GetString(_clientKeyIvDictionary[e.Message.From].Item2));
                                    //    break;
                                }
                                break;
                            case 1: //one-time-pad decryption
                                if (_clientPromtList.Find(cli => cli.Item1.UserName == e.Message.From).Item2.txtOneTimePad.Text.Length >= e.Message.MessageSent.Length)
                                    decryptedMessage = Cryptography.DecryptMessageWithPad(_clientPromtList.Find(cli => cli.Item1.UserName == e.Message.From).Item2.txtOneTimePad.Text, e.Message.MessageSent);
                                break;
                        }
                    }
                    else
                    {
                        decryptedMessage = Cryptography.DecryptData(e.Message.MessageSent);
                    }
                    _clientPromtList.Find(cli => cli.Item1.UserName == e.Message.From).Item2.RecieveText.AppendNewLine(String.Format("[{0}] >> {1}", DateTime.Now.ToString("HH:mm:ss"), decryptedMessage));
                }
                catch (Exception ex)
                {
                    _clientPromtList.Find(cli => cli.Item1.UserName == e.Message.From).Item2.RecieveText.AppendNewLine(String.Format("[{0}] >> {1}", DateTime.Now.ToString("HH:mm:ss"), "Could not decrypt incomming message."));
                }

            }));
        }

        private void ChatDialogoue_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            IMLibrary.Client selectedClient = (IMLibrary.Client) ListBuddies.Items[ListBuddies.SelectedIndex];
            if (selectedClient == null) return;
            if (e.Key != Key.Enter) return;

            e.Handled = true;
            if (_clientPromtList.Find(c => c.Item1.UserName == selectedClient.UserName).Item2.txtSend.Text == "") return;

            string message = _clientPromtList.Find(c => c.Item1 == selectedClient).Item2.txtSend.Text; /* Actual message that is to be sent, this is where we can encrypt it*/

            GeneratedClientList.ToList().Find(cli => cli.UserName == selectedClient.UserName).NewMessages = false;
            GeneratedClientList.ToList().Find(cli => cli.UserName == selectedClient.UserName).Received = 0;
            string encryptedMessage = "";
            try
            {
                if (_clientKeyIvDictionary.ContainsKey(selectedClient.UserName))
                {
                    int selectedEncryptionMethod = _clientPromtList.Find(c => c.Item1 == selectedClient).Item2.EncryptionMethod.SelectedIndex;
                    switch (selectedEncryptionMethod)
                    {
                        case 0: //symmetrical encryption
                            int selectedStandard = _clientPromtList.Find(c => c.Item1 == selectedClient).Item2.EncryptionStandard.SelectedIndex;
                            switch (selectedStandard)
                            {
                                case 0:
                                    encryptedMessage = Cryptography.Encrypt<AesManaged>(message, _clientKeyIvDictionary[selectedClient.UserName].Item1, Encoding.Unicode.GetString(_clientKeyIvDictionary[selectedClient.UserName].Item2));
                                    break;
                                case 1: //
                                    encryptedMessage = Cryptography.Encrypt<TripleDESCryptoServiceProvider>(message, _clientKeyIvDictionary[selectedClient.UserName].Item1, Encoding.Unicode.GetString(_clientKeyIvDictionary[selectedClient.UserName].Item2));
                                    break;
                                //case 2:
                                //    encryptedMessage = Cryptography.Encrypt<RC2CryptoServiceProvider>(message, _clientKeyIvDictionary[selectedClient.UserName].Item1, Encoding.Unicode.GetString(_clientKeyIvDictionary[selectedClient.UserName].Item2));
                                //    break;
                            }
                            break;
                        case 1: //one-time-pad
                            if (_clientPromtList.Find(c => c.Item1 == selectedClient).Item2.txtOneTimePad.Text.Length >= message.Length)
                                encryptedMessage = Cryptography.EncryptMessageWithPad(_clientPromtList.Find(c => c.Item1 == selectedClient).Item2.txtOneTimePad.Text, message);
                            break;
                    }
                }
                else
                {

                    encryptedMessage = Cryptography.EncryptData(message, selectedClient.PublicKey);
                }
                var isChecked = _clientPromtList.Find(c => c.Item1 == selectedClient).Item2.SynchroniseCheckBox.IsChecked;
                bool syncFlag = isChecked != null && (bool) isChecked;
                int encryptionStandardFlag = _clientPromtList.Find(c => c.Item1 == selectedClient).Item2.EncryptionStandard.SelectedIndex;
                int encryptionMethodFlag = _clientPromtList.Find(c => c.Item1 == selectedClient).Item2.EncryptionMethod.SelectedIndex;

                IMLibrary.Message messagePacket = new IMLibrary.Message(encryptedMessage, selectedClient.UserName, this._clientHandler.ClientInfo.UserName, encryptionStandardFlag, encryptionMethodFlag, syncFlag);
                _clientPromtList.Find(c => c.Item1 == selectedClient).Item2.RecieveText.AppendNewLine(String.Format("[{0}] << {1}", DateTime.Now.ToString("HH:mm:ss"), _clientPromtList.Find(c => c.Item1 == selectedClient).Item2.txtSend.Text));
                _clientPromtList.Find(c => c.Item1 == selectedClient).Item2.txtSend.Text = "";
                this._clientHandler.SendMessage(messagePacket);
            }
            catch
            {
                _clientPromtList.Find(c => c.Item1 == selectedClient).Item2.RecieveText.AppendNewLine(String.Format("[{0}] << {1}", DateTime.Now.ToString("HH:mm:ss"), "This user may have removed their public key from the server; this message cannot be sent to ensure security integrity."));
            }
        }




        private void ListBuddies_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == null) return;

            var item = ItemsControl.ContainerFromElement(ListBuddies, (DependencyObject) e.OriginalSource) as ListBoxItem;
            if (item == null) return;

            IMLibrary.Client selectedClient = (IMLibrary.Client) item.Content;
            gridBuddies.Children.Clear();
            _clientPromtList.Find(c => c.Item1 == selectedClient).Item2.PreviewKeyDown += ChatDialogoue_PreviewKeyDown;
            gridBuddies.Children.Add(_clientPromtList.Find(c => c.Item1 == selectedClient).Item2);
            GeneratedClientList.ToList().Find(cli => cli.UserName == selectedClient.UserName).Received = 0;
            GeneratedClientList.ToList().Find(cli => cli.UserName == selectedClient.UserName).NewMessages = false;
        }


        public Tuple<byte[], byte[]> GenerateUnencryptedKeySaltPair()
        {
            var cryptRng = System.Security.Cryptography.RandomNumberGenerator.Create();
            byte[] keyBytes = new byte[256];
            byte[] saltBytes = new byte[50];

            cryptRng.GetBytes(keyBytes);
            cryptRng.GetBytes(saltBytes);

            return new Tuple<byte[], byte[]>(keyBytes, saltBytes);
        }

        public Tuple<byte[], byte[]> EncryptKeySaltPair(RSAParameters key, Tuple<byte[], byte[]> unencryptedKeySaltPair)
        {
            string keyAsString = Encoding.Unicode.GetString(unencryptedKeySaltPair.Item1);
            string saltAsString = Encoding.Unicode.GetString(unencryptedKeySaltPair.Item2);

            string encryptedKeyAsString = Cryptography.EncryptData(keyAsString, key);
            string encryptedSaltAsString = Cryptography.EncryptData(saltAsString, key);

            byte[] encryptedKeyAsBytes = Encoding.Unicode.GetBytes(encryptedKeyAsString);
            byte[] encryptedSaltAsBytes = Encoding.Unicode.GetBytes(encryptedSaltAsString);

            return new Tuple<byte[], byte[]>(encryptedKeyAsBytes, encryptedSaltAsBytes);
        }

        private void TextBoxLocalDirectory_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchForclientTextBox.Text == "Search...")
                SearchForclientTextBox.Text = "";
        }

        private void TextBoxLocalDirectory_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchForclientTextBox.Text.Length == 0)
                SearchForclientTextBox.Text = "Search...";
        }

        private void SearchForclientTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_filteredView != null)
            {
                _filteredView.Filter = p => Search((IMLibrary.Client) p);
                _filteredView.Refresh();
            }
        }

        private void FirstButton_Click(object sender, RoutedEventArgs e)
        {
            _filteredView.Filter = null;
            SearchForclientTextBox.Text = "Search...";
            _filteredView.Refresh();
        }


        private bool Search(IMLibrary.Client client)
        {
            if (SearchForclientTextBox.Text == "Search...")
            {
                return true;
            }

            var bc = new BrushConverter();
            var colour = FavouriteButton.Background as SolidColorBrush;
            var colour2 = (System.Windows.Media.SolidColorBrush) bc.ConvertFrom("#FF038297");
            if (Equals(colour.Color, colour2.Color))
            {
                return (SearchForclientTextBox.Text != "" && client.UserName.Contains(SearchForclientTextBox.Text)) && GeneratedClientList.ToList().Single(cli => cli.UserName == _clientHandler.ClientInfo.UserName).Buddies.Contains(client.UserName);

            }

            return (SearchForclientTextBox.Text != "" && client.UserName.Contains(SearchForclientTextBox.Text));
        }

        private bool Buddies(IMLibrary.Client client)
        {
            if (GeneratedClientList.ToList().Single(cli => cli.UserName == _clientHandler.ClientInfo.UserName).Buddies == null)
                return false;

            return GeneratedClientList.ToList().Single(cli => cli.UserName == _clientHandler.ClientInfo.UserName).Buddies.Contains(client.UserName);
        }

        private void SecondButton_Click(object sender, RoutedEventArgs e)
        {
            SearchForclientTextBox.Text = "Search...";
            _filteredView.Filter = p => Buddies((IMLibrary.Client) p);
            _filteredView.Refresh();
        }


        private void AddBuddy(object sender, RoutedEventArgs e)
        {
            if (ListBuddies.SelectedIndex == -1) return;

            IMLibrary.Client selectedClient = (IMLibrary.Client) ListBuddies.SelectedItem;

            if (selectedClient != null)
            {
                this._clientHandler.Buddy(selectedClient.UserName, false);
            }
        }

        private void RemoveBuddy(object sender, RoutedEventArgs e)
        {
            if (ListBuddies.SelectedIndex == -1) return;

            IMLibrary.Client selectedClient = (IMLibrary.Client) ListBuddies.SelectedItem;

            if (selectedClient != null)
            {
                this._clientHandler.Buddy(selectedClient.UserName, true);
            }
        }

        private void ChatDialogue_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ListBuddies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }



        public void TestAesSpeed(object sender, RoutedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();

            string[] stringarray = new[]
            {
                "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.",
                "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout. The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English. Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy. Various versions have evolved over the years, sometimes by accident, sometimes on purpose (injected humour and the like).",
                "Contrary to popular belief, Lorem Ipsum is not simply random text. It has roots in a piece of classical Latin literature from 45 BC, making it over 2000 years old. Richard McClintock, a Latin professor at Hampden-Sydney College in Virginia, looked up one of the more obscure Latin words, consectetur, from a Lorem Ipsum passage, and going through the cites of the word in classical literature, discovered the undoubtable source. Lorem Ipsum comes from sections 1.10.32 and 1.10.33 of \"de Finibus Bonorum et Malorum\" (The Extremes of Good and Evil) by Cicero, written in 45 BC. This book is a treatise on the theory of ethics, very popular during the Renaissance. The first line of Lorem Ipsum, \"Lorem ipsum dolor sit amet..\", comes from a line in section 1.10.32.",
                "There are many variations of passages of Lorem Ipsum available, but the majority have suffered alteration in some form, by injected humour, or randomised words which don't look even slightly believable. If you are going to use a passage of Lorem Ipsum, you need to be sure there isn't anything embarrassing hidden in the middle of text. All the Lorem Ipsum generators on the Internet tend to repeat predefined chunks as necessary, making this the first true generator on the Internet. It uses a dictionary of over 200 Latin words, combined with a handful of model sentence structures, to generate Lorem Ipsum which looks reasonable. The generated Lorem Ipsum is therefore always free from repetition, injected humour, or non-characteristic words etc.",
                "At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non provident, similique sunt in culpa qui officia deserunt mollitia animi, id est laborum et dolorum fuga. Et harum quidem rerum facilis est et expedita distinctio. Nam libero tempore, cum soluta nobis est eligendi optio cumque nihil impedit quo minus id quod maxime placeat facere possimus, omnis voluptas assumenda est, omnis dolor repellendus. Temporibus autem quibusdam et aut officiis debitis aut rerum necessitatibus saepe eveniet ut et voluptates repudiandae sint et molestiae non recusandae. Itaque earum rerum hic tenetur a sapiente delectus, ut aut reiciendis voluptatibus maiores alias consequatur aut perferendis doloribus asperiores repellat."
            };

            string[] encryptedstrings = new string[5];
            long millieSecondsTotal = 0;
            long average = 0;
            int rounds = 0;
            while (rounds < 1000)
            {
                sw.Start();
                for (int i = 0; i < stringarray.Length; i++)
                {
                    encryptedstrings[i] = Cryptography.Encrypt<AesManaged>(stringarray[i], _clientKeyIvDictionary[_clientHandler.ClientInfo.UserName].Item1, Encoding.Unicode.GetString(_clientKeyIvDictionary[_clientHandler.ClientInfo.UserName].Item2));
                }
                sw.Stop();
                millieSecondsTotal += (sw.ElapsedMilliseconds / 5);
                sw.Reset();
                rounds++;
            }
            average = millieSecondsTotal/rounds;
            e.Handled = true;
        }


        public void TestDesSpeed(object sender, RoutedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();

            string[] stringarray = new[]
            {
                "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.",
                "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout. The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English. Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy. Various versions have evolved over the years, sometimes by accident, sometimes on purpose (injected humour and the like).",
                "Contrary to popular belief, Lorem Ipsum is not simply random text. It has roots in a piece of classical Latin literature from 45 BC, making it over 2000 years old. Richard McClintock, a Latin professor at Hampden-Sydney College in Virginia, looked up one of the more obscure Latin words, consectetur, from a Lorem Ipsum passage, and going through the cites of the word in classical literature, discovered the undoubtable source. Lorem Ipsum comes from sections 1.10.32 and 1.10.33 of \"de Finibus Bonorum et Malorum\" (The Extremes of Good and Evil) by Cicero, written in 45 BC. This book is a treatise on the theory of ethics, very popular during the Renaissance. The first line of Lorem Ipsum, \"Lorem ipsum dolor sit amet..\", comes from a line in section 1.10.32.",
                "There are many variations of passages of Lorem Ipsum available, but the majority have suffered alteration in some form, by injected humour, or randomised words which don't look even slightly believable. If you are going to use a passage of Lorem Ipsum, you need to be sure there isn't anything embarrassing hidden in the middle of text. All the Lorem Ipsum generators on the Internet tend to repeat predefined chunks as necessary, making this the first true generator on the Internet. It uses a dictionary of over 200 Latin words, combined with a handful of model sentence structures, to generate Lorem Ipsum which looks reasonable. The generated Lorem Ipsum is therefore always free from repetition, injected humour, or non-characteristic words etc.",
                "At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non provident, similique sunt in culpa qui officia deserunt mollitia animi, id est laborum et dolorum fuga. Et harum quidem rerum facilis est et expedita distinctio. Nam libero tempore, cum soluta nobis est eligendi optio cumque nihil impedit quo minus id quod maxime placeat facere possimus, omnis voluptas assumenda est, omnis dolor repellendus. Temporibus autem quibusdam et aut officiis debitis aut rerum necessitatibus saepe eveniet ut et voluptates repudiandae sint et molestiae non recusandae. Itaque earum rerum hic tenetur a sapiente delectus, ut aut reiciendis voluptatibus maiores alias consequatur aut perferendis doloribus asperiores repellat."
            };

            string[] encryptedstrings = new string[5];
            long millieSecondsTotal = 0;
            long average = 0;
            int rounds = 0;
            while (rounds < 1000)
            {
                sw.Start();
                for (int i = 0; i < stringarray.Length; i++)
                {
                    encryptedstrings[i] = Cryptography.Encrypt<TripleDESCryptoServiceProvider>(stringarray[i], _clientKeyIvDictionary[_clientHandler.ClientInfo.UserName].Item1, Encoding.Unicode.GetString(_clientKeyIvDictionary[_clientHandler.ClientInfo.UserName].Item2));
                }
                sw.Stop();
                millieSecondsTotal += (sw.ElapsedMilliseconds / 5);
                sw.Reset();
                rounds++;
            }
            average = millieSecondsTotal/rounds;
            e.Handled = true;
        }

        public void TestOneTimePad(object sender, RoutedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();

            string[] stringarray = new[]
            {
                "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.",
                "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout. The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English. Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy. Various versions have evolved over the years, sometimes by accident, sometimes on purpose (injected humour and the like).",
                "Contrary to popular belief, Lorem Ipsum is not simply random text. It has roots in a piece of classical Latin literature from 45 BC, making it over 2000 years old. Richard McClintock, a Latin professor at Hampden-Sydney College in Virginia, looked up one of the more obscure Latin words, consectetur, from a Lorem Ipsum passage, and going through the cites of the word in classical literature, discovered the undoubtable source. Lorem Ipsum comes from sections 1.10.32 and 1.10.33 of \"de Finibus Bonorum et Malorum\" (The Extremes of Good and Evil) by Cicero, written in 45 BC. This book is a treatise on the theory of ethics, very popular during the Renaissance. The first line of Lorem Ipsum, \"Lorem ipsum dolor sit amet..\", comes from a line in section 1.10.32.",
                "There are many variations of passages of Lorem Ipsum available, but the majority have suffered alteration in some form, by injected humour, or randomised words which don't look even slightly believable. If you are going to use a passage of Lorem Ipsum, you need to be sure there isn't anything embarrassing hidden in the middle of text. All the Lorem Ipsum generators on the Internet tend to repeat predefined chunks as necessary, making this the first true generator on the Internet. It uses a dictionary of over 200 Latin words, combined with a handful of model sentence structures, to generate Lorem Ipsum which looks reasonable. The generated Lorem Ipsum is therefore always free from repetition, injected humour, or non-characteristic words etc.",
                "At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non provident, similique sunt in culpa qui officia deserunt mollitia animi, id est laborum et dolorum fuga. Et harum quidem rerum facilis est et expedita distinctio. Nam libero tempore, cum soluta nobis est eligendi optio cumque nihil impedit quo minus id quod maxime placeat facere possimus, omnis voluptas assumenda est, omnis dolor repellendus. Temporibus autem quibusdam et aut officiis debitis aut rerum necessitatibus saepe eveniet ut et voluptates repudiandae sint et molestiae non recusandae. Itaque earum rerum hic tenetur a sapiente delectus, ut aut reiciendis voluptatibus maiores alias consequatur aut perferendis doloribus asperiores repellat."
            };

            string key = "On the other hand, we denounce with righteous indignation and dislike men who are so beguiled and demoralized by the charms of pleasure of the moment, so blinded by desire, that they cannot foresee the pain and trouble that are bound to ensue; and equal blame belongs to those who fail in their duty through weakness of will, which is the same as saying through shrinking from toil and pain.These cases are perfectly simple and easy to distinguish. In a free hour, when our power of choice is untrammelled and when nothing prevents our being able to do what we like best, every pleasure is to be welcomed and every pain avoided. But in certain circumstances and owing to the claims of duty or the obligations of business it will frequently occur that pleasures have to be repudiated and annoyances accepted.The wise man therefore always holds in these matters to this principle of selection: he rejects pleasures to secure other greater pleasures, or else he endures pains to avoid worse pains.";

            string[] encryptedstrings = new string[5];
            long millieSecondsTotal = 0;
            long average = 0;
            int rounds = 0;
            while (rounds < 1000)
            {
                sw.Start();
                for (int i = 0; i < stringarray.Length; i++)
                {
                    encryptedstrings[i] = Cryptography.EncryptMessageWithPad(key, stringarray[i]);
                }
                sw.Stop();
                millieSecondsTotal += (sw.ElapsedMilliseconds / 5);
                sw.Reset();
                rounds++;
            }
            average = millieSecondsTotal/rounds;
            e.Handled = true;


        }
    }
}



