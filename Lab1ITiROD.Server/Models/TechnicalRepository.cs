using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Lab1ITiROD.Common.Entity;
using Lab1ITiROD.Common.Repository;

namespace Lab1ITiROD.Server.Models
{
    public class TechnicalRepository : XmlRepository<Technical>
    {
        public TechnicalRepository(string path) : base(path)
        {
            if (!Path.EndsWith(".xml"))
            {
                throw new Exception("File not exist or has wrong format.");
            }
            else if (!File.Exists(Path))
            {
                XDocument document = new XDocument();
                XElement emptyTechnical = new XElement(nameof(Technical),
                    new XAttribute(nameof(Technical.Id), 1),
                    new XElement(nameof(Technical.Name), "Start"),
                    new XElement(nameof(Technical.StateName), "None"),
                    new XElement(nameof(Technical.Cost), 0),
                    new XElement(nameof(Technical.Weight), 0),
                    new XElement(nameof(Technical.Volume), 0));
                XElement root = new XElement("root");
                root.Add(emptyTechnical);
                document.Add(root);
                document.Save(Path);
            }
        }
        
        public override string Create(Technical item)
        {
                if (item == null)
                {
                    return "Message does not have item info";
                }
                XDocument file = XDocument.Load(Path);
                if (file.Root != null && file.Root.Elements().ToList().Any())
                {
                    var collection = file.Root.Elements().ToList();
                    bool flag = collection.Exists(x => GetItem(x).Name.Equals(item.Name));
                    if (!flag)
                    {
                        item.Id = GetId(collection.Last()) + 1;
                    }
                    else
                    {
                        return "Element is Exist";
                    }

                    item.Id = GetId(collection.Last()) + 1;
                    file.Root.Add(CreateElement(item));
                }
                else
                {
                    item.Id = 1;
                    file.AddFirst(CreateElement(item));
                }
                file.Save(Path);
                return "OK";
        }

        public override string Edit(Technical item)
        {
                if (item == null)
                {
                    return "Message does not have item info";
                }

                XDocument file = XDocument.Load(Path);
                if (file.Root != null && file.Root.Elements().Any(t => GetItem(t).Name.Equals(item.Name)))
                {
                    var collection = file.Root.Elements().Where(t => !GetItem(t).Name.Equals(item.Name)).ToList();
                    item.Id = Int32.Parse(file.Root.Elements().First(t => GetItem(t).Name.Equals(item.Name))
                        .Attribute(nameof(Technical.Id)).Value);
                    collection.Add(CreateElement(item));
                    file.Root.RemoveAll();
                    file.Root.Add(collection.OrderBy(GetId));
                    file.Save(Path);
                    return "OK";
                }

                return "Element not found.";
        }
        protected override XElement CreateElement(Technical item)
        {
            var xElement = new XElement(nameof(Technical),
                new XAttribute(nameof(Technical.Id), item.Id),
                new XElement(nameof(Technical.Name), item.Name),
                new XElement(nameof(Technical.StateName), item.StateName),
                new XElement(nameof(Technical.Cost), item.Cost),
                new XElement(nameof(Technical.Weight), item.Weight),
                new XElement(nameof(Technical.Volume), item.Volume));

            return xElement;
        }

        protected override Technical GetItem(XElement element)
        {
            var item = new Technical
            {
                Id = int.Parse(element.Attribute(nameof(Technical.Id)).Value),
                Name = element.Element(nameof(Technical.Name)).Value,
                StateName = element.Element(nameof(Technical.StateName)).Value,
                Cost = int.Parse(element.Element(nameof(Technical.Cost)).Value),
                Weight = int.Parse(element.Element(nameof(Technical.Weight)).Value),
                Volume = int.Parse(element.Element(nameof(Technical.Volume)).Value),
            };

            return item;
        }
    }
}