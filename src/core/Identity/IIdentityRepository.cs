using Fuxion.Identity;
using Fuxion.Repositories;

namespace Fuxion;

public interface IIdentityRepository : IKeyValueRepository<string, IIdentity> { }