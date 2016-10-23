using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using CoffeeJelly.tempa.Extensions;
using MsgPack.Serialization;

namespace CoffeeJelly.tempa
{
    public static partial class DataWorker
    {
        public static Task<List<T>> ReadBinaryAsync<T>(string path, string fileName) where T : ITermometer
        {
            return Task.Factory.StartNew(() => ReadBinary<T>(path, fileName));
        }

        public static List<T> ReadBinary<T>(string path, string fileName) where T : ITermometer
        {
            DatFileNameChecker(fileName);
            if (string.IsNullOrEmpty(path))
            throw new ArgumentException("path should have not be empty or null value.");

            List<T> termometerList;
            //BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(path.PathFormatter() + fileName, FileMode.OpenOrCreate))
            {
                //termometerList = (List<T>)formatter.Deserialize(fs);
                fs.Position = 0;
                var serializer = MessagePackSerializer.Get<List<T>>();
                termometerList = serializer.Unpack(fs);
            }
            return termometerList;
        }

        public static Task WriteBinaryAsync<T>(string path, string fileName, List<T> termometerList) where T : ITermometer
        {
            return Task.Factory.StartNew(() => WriteBinary<T>(path, fileName, termometerList));
        }

        public static void WriteBinary<T>(string path, string fileName, List<T> termometerList) where T : ITermometer
        {
            DatFileNameChecker(fileName);
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path should have not be empty or null value.");

            // BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(path.PathFormatter() + fileName, FileMode.Create))
            {
                //formatter.Serialize(fs, termometerList);
                var serializer = MessagePackSerializer.Get<List<T>>();
                serializer.Pack(fs, termometerList);
            }

        }

    }
}
