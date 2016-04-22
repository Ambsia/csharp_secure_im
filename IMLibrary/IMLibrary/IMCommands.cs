using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace IMLibrary
{
    public static class IMCommands
    {
        public const int IM_AUTHENTICATE = 8000;
        public const byte IM_SEND = 0;
        public const byte IM_RECEIVE = 1;
        public const byte IM_LOGIN = 2;
        public const byte IM_LOGOUT = 3;
        public const byte IM_REGISTER = 4;
        public const byte ERROR_IM_PASSWRONG = 5;
        public const byte ERROR_IM_USER_DOES_NOT_EXIST = 6;
        public const byte ERROR_IM_USER_EXSISTS = 7;
        public const byte ERROR_IM_USER_NOT_LOGGED_IN = 8;
        public const byte ERROR_IM_REGISTRATION_DEATILS_WRONG = 9;
        public const byte ERROR_UNKNOWN_COMMAND = 10;
        public const byte IM_READY = 11;
        public const byte ERROR_UNKNOWN_RECIPIENT = 12;
        public const byte IM_CLIENT_LIST = 13;
        public const byte IM_SETUP = 14;
        public const byte IM_PING = 15;
        public const byte IM_CORRECT = 50;
        public const byte ERROR_INCORRECT = 70;
        public const byte IM_RETRY = 90;
        public const byte ERROR_USER_ALREADY_LOGGED_IN = 16;
        public const byte IM_DISCONNECT = 17;
        public const byte PublicKey = 76;
        public const byte NegotiateKeys = 97;
        public const byte Verify = 98;
        public const byte CanForward = 96;
        public const byte MessageError = 95;
        public const byte Buddies = 68;
        public const byte AddBuddy = 54;
        public const byte RemoveBuddy = 55;


        public static readonly byte[] Finished = {54};
    }
}
