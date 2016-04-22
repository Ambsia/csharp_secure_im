using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace IMLibrary
{
    public static class StreamExtensions
    {

        public static byte[] ReadStreamTillEnd(this Stream stream, long byteArrayLength)
        {
            byte[] fileBuffer = new byte[byteArrayLength];

            int totalRead = 0;

            int dataFrames;
            while ((dataFrames = stream.Read(fileBuffer, totalRead, fileBuffer.Length - totalRead)) > 0)
            {
                totalRead += dataFrames;
                if (totalRead == fileBuffer.Length) //read what we expected, check for anymore incomming bytes
                {
                    int nextByteRead = stream.ReadByte();
                    if (nextByteRead == 54)
                    {
                        break;
                    }

                    //rezise buffer and place the new byte in the correct place
                    byte[] newFileBuffer = new byte[fileBuffer.Length * 2];
                    Array.Copy(fileBuffer, newFileBuffer, fileBuffer.Length);

                    Array.Copy(fileBuffer, newFileBuffer, fileBuffer.Length);
                    newFileBuffer[totalRead] = (byte)nextByteRead;
                    fileBuffer = newFileBuffer;
                    totalRead++;

                }
            }
            //here we should have bytes of the file
            //create the actual file, and copy it to disk
            var finalByteArray = new byte[totalRead];
            Array.Copy(fileBuffer, finalByteArray, totalRead);


            return finalByteArray;
        }

    }
}
