using Fuxion.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Fuxion.Identity.Functions;
namespace Fuxion.Identity.Test.Mocks
{
    class IdentityRepository : IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>
    {
        public bool Exist(string key) { return false; }
        public Task<bool> ExistAsync(string key) { return Task.FromResult(false); }
        public IIdentity Find(string key) {
            if(key == "root")
            {
                return new Identity
                {
                    Name = "root",
                    UserName = "root",
                    PasswordHash = new byte[] { 0x00 },
                    PasswordSalt = new byte[] { 0x00 },
                    Groups = new[]
                    {
                        new Group
                        {
                            Name = "rootGroup",
                            Permissions = new[]
                            {
                                new Permission(true,Read,new []
                                {
                                    new Scope(TypeDiscriminator.Create<Entity>(), ScopePropagation.ToMe | ScopePropagation.ToInclusions)
                                }),
                            }
                        }
                    },
                };
            }
            return null; }
        public Task<IIdentity> FindAsync(string key) { return Task.FromResult(Find(key)); }
        public IIdentity Get(string key) { return null; }
        public Task<IIdentity> GetAsync(string key) { return Task.FromResult<IIdentity>(null); }
        public void Remove(string key) { }
        public Task RemoveAsync(string key) { return Task.CompletedTask; }
        public void Set(string key, IIdentity value) { }
        public Task SetAsync(string key, IIdentity value) { return Task.CompletedTask; }
    }
}
