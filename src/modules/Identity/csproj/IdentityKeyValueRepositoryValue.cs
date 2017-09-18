using Fuxion.Repositories;

namespace Fuxion.Identity
{
    public class IdentityKeyValueRepositoryValue : IKeyValueEntry<string, IIdentity>
    {
        public string Key { get; set; }
        public IIdentity Value { get; set; }
    }
}
