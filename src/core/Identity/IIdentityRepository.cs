namespace Fuxion;

using Fuxion.Identity;
using Fuxion.Repositories;

public interface IIdentityRepository : IKeyValueRepository<string, IIdentity> { }