using System;

namespace Lab1ITiROD.Common.Entity
{
    [Serializable]
    public class Technical : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StateName { get; set; }
        public int Weight { get; set; }
        public int Volume { get; set; }
        public int Cost { get; set; }
    }
}