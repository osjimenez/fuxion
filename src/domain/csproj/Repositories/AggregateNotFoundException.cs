using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Domain.Repositories
{
    public class AggregateNotFoundException : Exception
    {
        public AggregateNotFoundException() { }
        public AggregateNotFoundException(Guid entityId) : base(entityId.ToString())
        {
            this.EntityId = entityId;
        }
        public AggregateNotFoundException(Guid entityId, string entityType)
            : base(entityType + ": " + entityId.ToString())
        {
            this.EntityId = entityId;
            this.EntityType = entityType;
        }
        public AggregateNotFoundException(Guid entityId, string entityType, string message, Exception inner)
            : base(message, inner)
        {
            this.EntityId = entityId;
            this.EntityType = entityType;
        }
        public Guid EntityId { get; private set; }
        public string EntityType { get; private set; }
    }
}
