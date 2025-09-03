using System;
using System.Threading.Tasks;

namespace ConsoleSearch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new App().RunAsync();
        }
    }
}
