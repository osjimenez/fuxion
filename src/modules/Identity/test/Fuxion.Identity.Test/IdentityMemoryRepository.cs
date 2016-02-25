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
using static Fuxion.Identity.Test.Context;
namespace Fuxion.Identity.Test
{
    public interface IIdentityTestRepository {
        IEnumerable<Album> Album { get; }
        IEnumerable<Song> Song { get; }
        IEnumerable<Circle> Circle { get; }
        IEnumerable<T> GetByType<T>();
    }
    public class IdentityMemoryTestRepository : IIdentityTestRepository, IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>
    {
        public IEnumerable<Album> Album { get { return Albums; } }
        public IEnumerable<Song> Song { get { return Songs; } }
        public IEnumerable<Circle> Circle { get { return Circles; } }

        public IEnumerable<T> GetByType<T>()
        {
            if (typeof(T) == typeof(Album)) return (IEnumerable<T>)Album;
            if (typeof(T) == typeof(Song)) return (IEnumerable<T>)Song;
            if (typeof(T) == typeof(Circle)) return (IEnumerable<T>)Circle;
            throw new KeyNotFoundException();
        }

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
