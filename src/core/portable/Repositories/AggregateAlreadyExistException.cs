using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Repositories
{
    public class AggregateAlreadyExistException : Exception
    {
        public AggregateAlreadyExistException() { }
        public AggregateAlreadyExistException(Guid entityId) : base(entityId.ToString())
        {
            this.EntityId = entityId;
        }
        public AggregateAlreadyExistException(Guid entityId, string entityType)
            : base(entityType + ": " + entityId.ToString())
        {
            this.EntityId = entityId;
            this.EntityType = entityType;
        }
        public AggregateAlreadyExistException(Guid entityId, string entityType, string message, Exception inner)
            : base(message, inner)
        {
            this.EntityId = entityId;
            this.EntityType = entityType;
        }
        public Guid EntityId { get; private set; }
        public string EntityType { get; private set; }
    }
}
