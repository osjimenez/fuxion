using Fuxion.ServiceModel.Description;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DemoWpf
{
    [ServiceContract(CallbackContract = typeof(IFactoryServiceCallback))]
    public interface IFactoryService
    {
        [OperationContract]
        void Echo();
        [OperationContract(IsOneWay = true)]
        void Ping();
    }
    public interface IFactoryServiceCallback
    {
        [OperationContract]
        void Pong();
    }
    [FactoryBehavior]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class FactoryService : IFactoryService
    {
        public FactoryService(ToInject toInject) { }
        public void Echo()
        {
            Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId} - WORKS !!!");
        }
        public void Ping()
        {
            Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId} - PING");
            OperationContext.Current.GetCallbackChannel<IFactoryServiceCallback>().Pong();
        }
    }
    public class ToInject
    {

    }
}
