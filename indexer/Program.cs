using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Indexer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Vælg database-mappe: large, medium eller small");
            string valg = Console.ReadLine();
            Config.SelectFolder(valg);
            new App().Run();
        }
        
    }
}