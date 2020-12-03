using System;
using System.Threading.Tasks;
using Lab1ITiROD.Common.Entity;
using Lab1ITiROD.Server.Models;

namespace Lab1ITiROD.Server
{
    static class Program
    {
        private const string Ip = "127.0.0.1";
        private const int Port = 25565;
        private const string Path = "..\\Files\\Technical.xml";

        static async Task Main()
        {
            ServerFacade<Technical> server = new ServerFacade<Technical>(new TechnicalRepository(Path), Ip, Port);

            await server.Start();

            Console.ReadKey();
        }
    }
}