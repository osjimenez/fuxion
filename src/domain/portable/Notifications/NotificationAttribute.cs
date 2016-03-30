using System;
namespace Fuxion.Domain.Notifications
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NotificationAttribute : Attribute
    {
        public NotificationAttribute(Type modelType) { ModelType = modelType; }
        public Type ModelType { get; set; }
    }
}
