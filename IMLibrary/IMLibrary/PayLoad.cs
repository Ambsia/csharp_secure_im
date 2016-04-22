using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
namespace IMLibrary
{
    public static class PayLoad
    {
        public static byte[] ObjectToByte(this object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
                serializer.WriteObject(memoryStream, obj);
                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        public static object ByteStreamToObject(this byte[] packet, Type t)
        {
            using (Stream stream = new MemoryStream())
            {
                stream.Write(packet, 0, packet.Length);
                stream.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(t);

                return deserializer.ReadObject(stream);
            }
        }

    }
}
