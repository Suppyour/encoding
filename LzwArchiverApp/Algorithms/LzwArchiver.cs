using System;
using System.Collections.Generic;
using System.IO;

namespace LzwArchiverApp.Algorithms
{
    public static class LzwArchiver
    {
        private const int MaxDictSize = 65536; // 16-bit размер

        public static void Compress(string inputPath, string outputPath)
        {
            byte[] uncompressed = File.ReadAllBytes(inputPath);
            if (uncompressed.Length == 0)
            {
                File.WriteAllBytes(outputPath, new byte[0]);
                return;
            }

            int dictSize = 256;
            var dictionary = new Dictionary<(int, byte), int>();
            int w = -1;
            var compressed = new List<ushort>();

            foreach (byte k in uncompressed)
            {
                if (w == -1)
                {
                    w = k;
                }
                else
                {
                    if (dictionary.ContainsKey((w, k)))
                    {
                        w = dictionary[(w, k)];
                    }
                    else
                    {
                        compressed.Add((ushort)w);
                        if (dictSize < MaxDictSize)
                        {
                            dictionary.Add((w, k), dictSize++);
                        }
                        w = k;
                    }
                }
            }

            if (w != -1)
            {
                compressed.Add((ushort)w);
            }

            using (var fs = new FileStream(outputPath, FileMode.Create))
            using (var bw = new BinaryWriter(fs))
            {
                foreach (var code in compressed)
                {
                    bw.Write(code);
                }
            }
        }

        public static void Decompress(string inputPath, string outputPath)
        {
            byte[] fileBytes = File.ReadAllBytes(inputPath);
            if (fileBytes.Length == 0)
            {
                File.WriteAllBytes(outputPath, new byte[0]);
                return;
            }

            var compressed = new List<ushort>();
            for (int i = 0; i < fileBytes.Length; i += 2)
            {
                compressed.Add(BitConverter.ToUInt16(fileBytes, i));
            }

            int dictSize = 256;
            var dictionary = new Dictionary<int, byte[]>();
            for (int i = 0; i < 256; i++)
            {
                dictionary[i] = new byte[] { (byte)i };
            }

            int w = compressed[0];
            var decompressed = new List<byte>();
            
            decompressed.AddRange(dictionary[w]);

            for (int i = 1; i < compressed.Count; i++)
            {
                int k = compressed[i];
                byte[] entry;
                if (dictionary.ContainsKey(k))
                {
                    entry = dictionary[k];
                }
                else if (k == dictSize)
                {
                    entry = new byte[dictionary[w].Length + 1];
                    dictionary[w].CopyTo(entry, 0);
                    entry[entry.Length - 1] = dictionary[w][0];
                }
                else
                {
                    throw new Exception("Ошибка алгоритма распаковки: неверный код в потоке.");
                }

                decompressed.AddRange(entry);

                if (dictSize < MaxDictSize)
                {
                    byte[] newEntry = new byte[dictionary[w].Length + 1];
                    dictionary[w].CopyTo(newEntry, 0);
                    newEntry[newEntry.Length - 1] = entry[0];
                    dictionary.Add(dictSize++, newEntry);
                }
                w = k;
            }

            File.WriteAllBytes(outputPath, decompressed.ToArray());
        }
    }
}
