using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMServer
{
    public class ConsoleBuffer
    {
        private static readonly List<string> ServerLog  = new List<string>();
        private static int _positionInLog = 0;
        public static void PrintLine(string text)
        {
            ServerLog.Add("[" + DateTime.Now + "] " + text);
            Console.WriteLine(ServerLog[_positionInLog++]);
        }

        internal static void PrintUserDetails(ClientInformation clientInfo)
        {
            PrintLine("Username = " + clientInfo.Username);
            PrintLine("Password = " + clientInfo.Password);
            PrintLine("Salt = " + clientInfo.Salt);
            PrintLine("Available = " + clientInfo.LoggedIn);
        }
    }
}
