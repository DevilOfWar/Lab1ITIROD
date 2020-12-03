using System.Threading.Tasks;
using Lab1ITiROD.Client.Services;

namespace Lab1ITiROD.Client
{
    static class Program
    {
        private const string Ip = "127.0.0.1";
        private const int Port = 25565;
        static async Task Main()
        {
            ClientService client = new ClientService(Ip, Port);
            await client.Start();
        }
    }
}