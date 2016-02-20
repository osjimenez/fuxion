using Fuxion.Identity.Test.Entity;
using Fuxion.Identity.Test.Helpers;
using Fuxion.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Fuxion.Identity.Functions;
using System.Collections;
using static Fuxion.Identity.Test.StaticContext;
namespace Fuxion.Identity.Test
{
    
    public class IdentityMemoryRepository : IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>
    {
        #region Lists

        #endregion
        public bool Exist(string key) { return false; }
        public Task<bool> ExistAsync(string key) { return Task.FromResult(false); }
        public IIdentity Find(string key) { return Identities.FirstOrDefault(i => i.UserName == key); }
        public Task<IIdentity> FindAsync(string key) { return Task.FromResult(Find(key)); }
        public IIdentity Get(string key) { return null; }
        public Task<IIdentity> GetAsync(string key) { return Task.FromResult<IIdentity>(null); }
        public void Remove(string key) { }
        public Task RemoveAsync(string key) { return Task.CompletedTask; }
        public void Set(string key, IIdentity value) { }
        public Task SetAsync(string key, IIdentity value) { return Task.CompletedTask; }
    }
}
