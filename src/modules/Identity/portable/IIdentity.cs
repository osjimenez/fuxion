using System;
using System.Collections.Generic;
using System.Linq;
using Fuxion.Identity.Helpers;
namespace Fuxion.Identity
{
    public interface IIdentity : IRol
    {
        string UserName { get; }
        byte[] PasswordHash { get; }
        byte[] PasswordSalt { get; }
    }
}
