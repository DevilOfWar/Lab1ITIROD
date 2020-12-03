using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Lab1ITiROD.Common.Entity;
using Lab1ITiROD.Common.Repository;

namespace Lab1ITiROD.Server.Models
{
    public class ServerFacade<T>
    where T: class, IEntity
    {
        private readonly IRepository<T> _repository;
        private readonly BinaryFormatter _formatter = new BinaryFormatter();
        private int _threadCount;
        private readonly object _locker = new object();
        private static TcpListener _server;

        private class ProcessingParams
        {
            public NetworkStream Stream { get; }
            public DataContainer<T> OperationData { get; }

            public ProcessingParams(DataContainer<T> operationData, NetworkStream stream)
            {
                Stream = stream;
                OperationData = operationData;
            }
        }

        public ServerFacade(IRepository<T> repository, string ip, int port)
        {
            _repository = repository;
            _server = new TcpListener(IPAddress.Parse(ip), port);
        }

        public async Task Start()
        {
            try
            {
                _server.Start();
                Console.WriteLine("Start Server...");
                while (true)
                {
                    if (_threadCount < 5)
                    {
                        Thread thread = new Thread(ClientCatching);
                        _threadCount++;
                        thread.Start();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await using StreamWriter stream = new StreamWriter("log.txt", true);
                await stream.WriteLineAsync(e.Message);
                throw;
            }
        }

        private async void ClientCatching()
        {
            TcpClient client = await _server.AcceptTcpClientAsync();
            Console.WriteLine("New connection is found...");
            await using NetworkStream stream = client.GetStream();
            DataContainer<T> operation = _formatter.Deserialize(stream) as DataContainer<T>;
           ProccessOperation(new ProcessingParams(operation, stream));
           _threadCount--;
        }
        
        private void ProccessOperation(object param)
        {
            if (!(param is ProcessingParams))
            {
                using (StreamWriter log = new StreamWriter("log.txt", true))
                {
                    log.WriteLine("Bad message format.");
                }
                return;
            }
            DataContainer<T> operation = ((ProcessingParams) param).OperationData;
            NetworkStream stream = ((ProcessingParams) param).Stream;
            lock (_locker)
            {
                switch (operation.Operation)
                {
                    case Operation.Create:
                    {
                        var result = _repository.Create(operation.Data);
                        _formatter.Serialize(stream, result);
                        using (var log = new StreamWriter("log.txt", true))
                        {
                            log.WriteLine("Result of creating: " + result + ".");
                        }
                        break;
                    }
                    case Operation.Delete:
                    {
                        var result = _repository.Delete(operation.Data);
                        _formatter.Serialize(stream, result);
                        using (var log = new StreamWriter("log.txt", true))
                        {
                            log.WriteLine("Result of deleting: " + result + ".");
                        }
                        break;
                    }
                    case Operation.Edit:
                    {
                        var result = _repository.Edit(operation.Data);
                        _formatter.Serialize(stream, result);
                        using (var log = new StreamWriter("log.txt", true))
                        {
                            log.WriteLine("Result of editing: " + result + ".");
                        }
                        break;
                    }
                    case Operation.Watch:
                    {
                        var outData = _repository.Watch();
                        _formatter.Serialize(stream, outData);
                        using (var log = new StreamWriter("log.txt", true))
                        {
                            log.WriteLine("Database is read.");
                        }
                        break;
                    }
                    default:
                        _formatter.Serialize(stream, "Wrong operation type.");
                        break;
                }
            }
            Console.WriteLine("Someone disconnected...");
        }
    }
}