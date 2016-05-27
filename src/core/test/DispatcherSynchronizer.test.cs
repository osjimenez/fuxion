using Fuxion.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test
{
    public class DispatcherSynchronizerTest
    {
        public DispatcherSynchronizerTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        [Fact]
        public void DispatcherSynchronizer_First()
        {
            NotifierMock not = new NotifierMock();

        }
    }
    //public class NotifierMock : Notifier<NotifierMock>
    //{

    //}
}
