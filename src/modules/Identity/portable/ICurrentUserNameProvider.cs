using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity
{
    public interface ICurrentUserNameProvider
    {
        string GetCurrentUserName();
    }
    public class GenericCurrentUserNameProvider : ICurrentUserNameProvider
    {
        public GenericCurrentUserNameProvider(Func<string> getCurrentUsernameFunction) { this.getCurrentUsernameFunction = getCurrentUsernameFunction; }
        Func<string> getCurrentUsernameFunction;
        public string GetCurrentUserName() { return getCurrentUsernameFunction(); }
    }
}
