using System;

namespace CORE.APP.Domain
{
    /// <summary>
    /// Abstract base class for all entities.
    /// </summary>
    public abstract class Entity
    {
        public int Id { get; set; }

        public string Guid { get; set; }
    }
}