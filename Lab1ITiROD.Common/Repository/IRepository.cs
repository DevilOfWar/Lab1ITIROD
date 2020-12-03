using System.Collections.Generic;
using Lab1ITiROD.Common.Entity;

namespace Lab1ITiROD.Common.Repository
{
    public interface IRepository<T>
    where T: class, IEntity
    {
        string Create(T item);
        string Edit(T item);
        string Delete(T item);
        IEnumerable<T> Watch();
    }
}