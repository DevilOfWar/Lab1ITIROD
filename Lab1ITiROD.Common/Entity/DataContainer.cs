using System;

namespace Lab1ITiROD.Common.Entity
{
    [Serializable]
    public class DataContainer<T>
    {
        public T Data { get; set; }
        public Operation Operation { get; set; }
    }
}