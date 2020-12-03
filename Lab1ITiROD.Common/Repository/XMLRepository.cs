using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Lab1ITiROD.Common.Entity;

namespace Lab1ITiROD.Common.Repository
{
    public abstract class XmlRepository<T> : IRepository<T>
    where T: class, IEntity
    {
        protected readonly string Path;

        protected XmlRepository(string path)
        {
            Path = path;
        }
        public virtual string Create(T item)
        {
                if (item == null)
                {
                    return "Message does not have item info";
                }
                XDocument file = XDocument.Load(Path);
                if (file.Root != null && file.Root.Elements().ToList().Any())
                {
                    var collection = file.Root.Elements().ToList();
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
        protected int GetId(XElement element)
        {
            return int.Parse(element.Attribute(nameof(IEntity.Id)).Value);
        }

        protected abstract XElement CreateElement(T item);
        public abstract string Edit(T item);

        public string Delete(T item)
        {
                if (item == null)
                {
                    return "Message does not have item info";
                }
                XDocument file = XDocument.Load(Path);
                if (file.Root != null && file.Root.Elements().Any(t => GetId(t) == item.Id))
                {
                    var collection = file.Root.Elements().Where(t => GetId(t) != item.Id).ToList();
                    file.Root.RemoveAll();
                    file.Root.Add(collection.OrderBy(GetId));
                    file.Save(Path);
                    return "OK";
                }
                return "Element not found.";
        }
        public IEnumerable<T> Watch()
        {
            var collection = new List<T>();
                XDocument file = XDocument.Load(Path);
                if (file.Root != null)
                {
                    foreach (XElement element in file.Root.Elements())
                    {
                        collection.Add(GetItem(element));
                    }
                }
            return collection;
        }
        protected abstract T GetItem(XElement element);
    }
}