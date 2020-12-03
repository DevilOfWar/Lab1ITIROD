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
        private readonly string _ip;
        private readonly int _port;
        private readonly BinaryFormatter _formatter = new BinaryFormatter();
        private List<Thread> _threadList = new List<Thread>();
        private List<TcpClient> _clientList = new List<TcpClient>();
        private readonly object _locker = new object();

        private class ProcessingParams
        {
            public TcpClient Client { get; }
            public DataContainer<T> OperationData { get; }

            public ProcessingParams(DataContainer<T> operationData, TcpClient client)
            {
                Client = client;
                OperationData = operationData;
            }
        }

        public ServerFacade(IRepository<T> repository, string ip, int port)
        {
            _repository = repository;
            _ip = ip;
            _port = port;
        }

        public async Task Start()
        {
            try
            {
                TcpListener server = new TcpListener(IPAddress.Parse(_ip), _port);
                server.Start();
                while (true)
                {
                    _clientList.Add(await server.AcceptTcpClientAsync());
                    Console.WriteLine("New connection is found...");
                    await using NetworkStream stream = _clientList.Last().GetStream();
                    DataContainer<T> operation = _formatter.Deserialize(stream) as DataContainer<T>;
                    _threadList.Add(new Thread(ProccessOperation));
                    _threadList.Last().Start(new ProcessingParams(operation, _clientList.Last()));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await using StreamWriter stream = new StreamWriter("log.txt", true);
                await stream.WriteAsync(e.Message);
                throw;
            }
        }

        private async void ProccessOperation(object param)
        {
            if (!(param is ProcessingParams))
            {
                await using StreamWriter log = new StreamWriter("log.txt", true);
                await log.WriteAsync("Bad message format.");
                return;
            }
            DataContainer<T> operation = ((ProcessingParams) param).OperationData;
            NetworkStream stream = ((ProcessingParams) param).Client.GetStream();
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
                            log.Write("Result of creating: " + result + ".");
                        }
                        break;
                    }
                    case Operation.Delele:
                    {
                        var result = _repository.Delete(operation.Data);
                        _formatter.Serialize(stream, result);
                        using (var log = new StreamWriter("log.txt", true))
                        {
                            log.Write("Result of deleting: " + result + ".");
                        }
                        break;
                    }
                    case Operation.Edit:
                    {
                        var result = _repository.Edit(operation.Data);
                        _formatter.Serialize(stream, result);
                        using (var log = new StreamWriter("log.txt", true))
                        {
                            log.Write("Result of editing: " + result + ".");
                        }
                        break;
                    }
                    case Operation.Watch:
                    {
                        var outData = _repository.Watch();
                        _formatter.Serialize(stream, outData);
                        using (var log = new StreamWriter("log.txt", true))
                        {
                            log.Write("Database is read.");
                        }
                        break;
                    }
                    default:
                        _formatter.Serialize(stream, "Wrong operation type.");
                        break;
                }
            }

            _clientList.Remove(((ProcessingParams) param).Client);
            _threadList.Remove(Thread.CurrentThread);
            Console.WriteLine("Someone disconnected...");
        }
    }
}