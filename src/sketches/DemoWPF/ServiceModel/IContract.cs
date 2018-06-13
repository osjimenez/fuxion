using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DemoWpf.ServiceModel
{
	[ServiceContract(CallbackContract = typeof(ICallback))]
	public interface IContract
	{
		[OperationContract]
		void Test();
	}
	public interface ICallback
	{
		[OperationContract]
		void CallbackTest();
	}
}
