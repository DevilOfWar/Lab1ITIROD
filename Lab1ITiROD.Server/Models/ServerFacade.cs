using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private static string _ip;
        private static int _port;
        private readonly BinaryFormatter _formatter = new BinaryFormatter();
        private List<Thread> _threadList = new List<Thread>();
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
            _ip = ip;
            _port = port;
            _server = new TcpListener(IPAddress.Parse(_ip), _port);
        }

        public async Task Start()
        {
            try
            {
                _server.Start();
                Console.WriteLine("Start Server...");
                while (true)
                {
                    if (_threadList.Count < 5)
                    {
                        _threadList.Add(new Thread(ClientCatching));
                        _threadList.Last().Start();
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
            using (StreamWriter log = new StreamWriter("log.txt", true))
            {
                log.WriteLine("Stream Writable: " + stream.CanWrite);
            }
            DataContainer<T> operation = _formatter.Deserialize(stream) as DataContainer<T>;
           ProccessOperation(new ProcessingParams(operation, stream));
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
            using (StreamWriter log = new StreamWriter("log.txt", true))
            {
                log.WriteLine("Stream Writable: " + stream.CanWrite);
            }
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
                    case Operation.Delele:
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
            _threadList.Remove(Thread.CurrentThread);
            Console.WriteLine("Someone disconnected...");
        }
    }
}