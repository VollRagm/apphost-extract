using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            // this originally was meant to test the extractor and now turned into the hash generator
            Console.WriteLine("Generating hashes.txt...");
            var existing = File.ReadAllLines("hashes.txt").ToList();
            var files = Directory.GetFiles(".\\files");
            SHA256Managed sha = new SHA256Managed();
            foreach (var file in files)
            {
                try
                {
                    var hash = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(file))).Replace("-", "");
                    if (existing.Contains(hash))
                    {
                        Console.WriteLine(file + " is known");
                        continue;
                    }
                    File.AppendAllText("hashes.txt", hash + "\n");
                    existing.Add(hash);
                }
                catch
                {
                    Console.WriteLine("exception lol");
                }
            }

        }

    }
}
