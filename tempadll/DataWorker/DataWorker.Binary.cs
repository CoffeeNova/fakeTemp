﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using CoffeeJelly.tempadll.Extensions;
using MsgPack.Serialization;
using CoffeeJelly.tempadll;

namespace CoffeeJelly.tempadll
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
            using (FileStream fs = new FileStream(path.PathFormatter() + fileName, FileMode.OpenOrCreate))
            {
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

            using (FileStream fs = new FileStream(path.PathFormatter() + fileName, FileMode.Create))
            {
                var serializer = MessagePackSerializer.Get<List<T>>();
                serializer.Pack(fs, termometerList);
            }
        }

    }
}
