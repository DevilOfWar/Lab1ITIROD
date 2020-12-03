using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Lab1ITiROD.Common.Entity;

namespace Lab1ITiROD.Client.Services
{
    public class ClientService
    {
        private readonly string _ip;
        private readonly int _port;
        private readonly BinaryFormatter _formatter = new BinaryFormatter();
        public ClientService(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public async Task Start()
        {
            bool flag = true;
            while (flag)
            {
                Console.WriteLine("1. Watch all Technical.");
                Console.WriteLine("2. Create new Technical.");
                Console.WriteLine("3. Edit Technical.");
                Console.WriteLine("4. Delete Technical.");
                Console.WriteLine("5. Find Technical by cost");
                Console.WriteLine("6. Sort by weight.");
                Console.WriteLine("7. Exit");
                string line = Console.ReadLine();
                switch (line)
                {
                    case "1":
                    {
                        using TcpClient client = new TcpClient(_ip, _port);
                        await using NetworkStream stream = client.GetStream();
                        DataContainer<Technical> sendData = new DataContainer<Technical> {Operation = Operation.Watch};
                        _formatter.Serialize(stream, sendData);
                        if (_formatter.Deserialize(stream) is List<Technical> recieveData)
                        {
                            foreach (var item in recieveData)
                            {
                                Console.WriteLine("Name: " + item.Name + "; State of Product: " + item.StateName +
                                                  "; Cost: " + item.Cost + "by.; Weight: " + item.Weight +
                                                  "kg.; Volume: " + item.Volume + "m^3.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Collection is empty");
                        }
                        break;
                    }
                    case "2":
                    {
                        int convert;
                        Console.Write("Enter name of Technical: ");
                        string name = Console.ReadLine();
                        Console.Write("Enter state name, where technical is created: ");
                        string stateName = Console.ReadLine();
                        Console.Write("Enter cost of Technical (default is 0): ");
                        line = Console.ReadLine();
                        int cost;
                        if (line == null || Int32.TryParse(line, out convert))
                        {
                            cost = line == null ? 0 : Int32.Parse(line);
                        }
                        else break;
                        Console.Write("Enter weight of Technical (default is 0): ");
                        line = Console.ReadLine();
                        int weight;
                        if (line == null || Int32.TryParse(line, out convert))
                        {
                            weight = line == null ? 0 : Int32.Parse(line);
                        }
                        else break;
                        Console.Write("Enter volume of Technical (default is 0): ");
                        line = Console.ReadLine();
                        int volume;
                        if (line == null || Int32.TryParse(line, out convert))
                        {
                            volume = line == null ? 0 : Int32.Parse(line);
                        }
                        else break;
                        Technical newTechnical = new Technical()
                        {
                            Cost = cost,
                            Name = name,
                            StateName = stateName,
                            Volume = volume,
                            Weight = weight
                        };
                        using TcpClient client = new TcpClient(_ip, _port);
                        await using NetworkStream stream = client.GetStream();
                        DataContainer<Technical> sendData = new DataContainer<Technical>()
                        {
                            Data = newTechnical,
                            Operation = Operation.Create
                        };
                        _formatter.Serialize(stream, sendData);
                        string recieveData = _formatter.Deserialize(stream) as string;
                        if (recieveData == null)
                        {
                            Console.WriteLine("Creating failed.");
                        }
                        else if (recieveData.Equals("OK"))
                        {
                            Console.WriteLine("Creating successful.");
                        }
                        else if (recieveData.Equals("Exist"))
                        {
                            Console.WriteLine("Technical already exist with tipped name.");
                        }
                        else
                        {
                            Console.WriteLine("Creating failed.");
                        }

                        break;
                    }
                    case "3":
                    {
                        int convert;
                        Console.Write("Enter name of Technical: ");
                        string name = Console.ReadLine();
                        Console.Write("Enter state name, where technical is created: ");
                        string stateName = Console.ReadLine();
                        Console.Write("Enter cost of Technical (default is 0): ");
                        line = Console.ReadLine();
                        int cost;
                        if (line == null || Int32.TryParse(line, out convert))
                        {
                            cost = line == null ? 0 : Int32.Parse(line);
                        }
                        else break;
                        Console.Write("Enter weight of Technical (default is 0): ");
                        line = Console.ReadLine();
                        int weight;
                        if (line == null || Int32.TryParse(line, out convert))
                        {
                            weight = line == null ? 0 : Int32.Parse(line);
                        }
                        else break;
                        Console.Write("Enter volume of Technical (default is 0): ");
                        line = Console.ReadLine();
                        int volume;
                        if (line == null || Int32.TryParse(line, out convert))
                        {
                            volume = line == null ? 0 : Int32.Parse(line);
                        }
                        else break;
                        Technical newTechnical = new Technical()
                        {
                            Cost = cost,
                            Name = name,
                            StateName = stateName,
                            Volume = volume,
                            Weight = weight
                        };
                        using TcpClient client = new TcpClient(_ip, _port);
                        await using NetworkStream stream = client.GetStream();
                        DataContainer<Technical> sendData = new DataContainer<Technical>
                        {
                            Operation = Operation.Edit,
                            Data = newTechnical
                        };
                        _formatter.Serialize(stream, sendData);
                        string recieveData = _formatter.Deserialize(stream) as string;
                        if (recieveData == null)
                        {
                            Console.WriteLine("Editting failed");
                        }
                        else
                        {
                            Console.WriteLine(recieveData.Equals("OK") ? "Editting successful." : "Editting failed.");
                        }
                        break;
                    }
                    case "4":
                    {
                        Console.Write("Enter name of Technical: ");
                        string name = Console.ReadLine();
                        Technical newTechnical = new Technical()
                        {
                            Name = name
                        };
                        using TcpClient clientWatch = new TcpClient(_ip, _port);
                        await using NetworkStream streamWatch = clientWatch.GetStream();
                        DataContainer<Technical> sendData = new DataContainer<Technical> {Operation = Operation.Watch};
                        _formatter.Serialize(streamWatch, sendData);
                        List<Technical> recieveDataList = _formatter.Deserialize(streamWatch) as List<Technical>;
                        if (recieveDataList.First(x => x.Name.Equals(newTechnical.Name)) != null)
                        {
                            using TcpClient clientDelete = new TcpClient(_ip, _port);
                            await using NetworkStream streamDelete = clientDelete.GetStream();
                            newTechnical = recieveDataList.First(x => x.Name.Equals(newTechnical.Name));
                            sendData = new DataContainer<Technical>()
                            {
                                Data = newTechnical,
                                Operation = Operation.Delele
                            };
                            _formatter.Serialize(streamDelete, sendData);
                            string recieveData = _formatter.Deserialize(streamDelete) as string;
                            Console.WriteLine(recieveData.Equals("OK") ? "Deleting successful." : "Deleting failed.");
                        }
                        else
                        {
                            Console.WriteLine("Technical with this name is not existed.");
                        }

                        break;
                    }
                    case "5":
                    {
                        Console.Write("Enter cost of Technical: ");
                        int cost = Int32.Parse((Console.ReadLine()));
                        using TcpClient client = new TcpClient(_ip, _port);
                        await using NetworkStream stream = client.GetStream();
                        DataContainer<Technical> sendData = new DataContainer<Technical> {Operation = Operation.Watch};
                        _formatter.Serialize(stream, sendData);
                        List<Technical> recieveData = _formatter.Deserialize(stream) as List<Technical>;
                        if (recieveData.First(x => x.Cost == cost) != null)
                        {
                            foreach (Technical item in recieveData.FindAll(x => x.Cost == cost))
                            {
                                Console.WriteLine("Name: " + item.Name + "; State of Product: " + item.StateName + 
                                                  "; Cost: " + item.Cost + "by.; Weight: " + item.Weight +
                                                  "kg.; Volume: " + item.Volume + "m^3.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Technical with this cost is not existed.");
                        }

                        break;
                    }
                    case "6":
                    {
                        using TcpClient client = new TcpClient(_ip, _port);
                        await using NetworkStream stream = client.GetStream();
                        DataContainer<Technical> sendData = new DataContainer<Technical> {Operation = Operation.Watch};
                        _formatter.Serialize(stream, sendData);
                        List<Technical> recieveData = _formatter.Deserialize(stream) as List<Technical>;
                        recieveData.Sort((x, y) =>
                        {
                            if (x.Weight > y.Weight)
                            {
                                return 1;
                            }

                            if (y.Weight > x.Weight)
                            {
                                return -1;
                            }
                            return 0;
                        });
                        foreach (Technical item in recieveData)
                        {
                            Console.WriteLine("Name: " + item.Name + "; State of Product: " + item.StateName + 
                                              "; Cost: " + item.Cost + "by.; Weight: " + item.Weight +
                                              "kg.; Volume: " + item.Volume + "m^3.");
                        }

                        break;
                    }
                    case "7":
                    {
                        flag = false;
                        break;
                    }
                    default:
                    {
                        Console.WriteLine("Wrong input.");
                        break;
                    }
                }
                Console.WriteLine("Enter any key to continue...");
                Console.ReadKey();
            }
        }
    }
}