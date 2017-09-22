using Fuxion.Identity.Test.Dao;
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
namespace Fuxion.Identity.Test.Repositories
{
    public interface IIdentityTestRepository {
        IEnumerable<AlbumDao> Album { get; }
        IEnumerable<SongDao> Song { get; }
        //IEnumerable<Circle> Circle { get; }
        IEnumerable<GroupDao> Group { get; }
        IEnumerable<DocumentDao> Document { get; }
        IEnumerable<T> GetByType<T>();
    }
    public class IdentityMemoryTestRepository : IIdentityTestRepository, IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>
    {
        public IEnumerable<AlbumDao> Album { get { return Context.File.Package.Album.GetAll(); } }
        public IEnumerable<SongDao> Song { get { return Context.File.Media.Song.GetAll(); } }
        //public IEnumerable<Circle> Circle { get { return Circles; } }
        public IEnumerable<GroupDao> Group { get { return Context.Rols.Group.GetAll(); } }
        public IEnumerable<DocumentDao> Document { get { return Context.File.Document.GetAll(); } }

        public IEnumerable<T> GetByType<T>()
        {
            if (typeof(T) == typeof(AlbumDao))  return (IEnumerable<T>)Album;
            if (typeof(T) == typeof(SongDao))   return (IEnumerable<T>)Song;
            //if (typeof(T) == typeof(Circle)) return (IEnumerable<T>)Circle;
            if (typeof(T) == typeof(GroupDao))  return (IEnumerable<T>)Group;
            throw new KeyNotFoundException();
        }

        public bool Exist(string key) { return false; }
        public Task<bool> ExistAsync(string key) { return Task.FromResult(Exist(key)); }
        public IIdentity Find(string key) { return Context.Rols.Identity.GetAll().FirstOrDefault(i => i.UserName == key); }
        public Task<IIdentity> FindAsync(string key) { return Task.FromResult(Find(key)); }
        public IIdentity Get(string key) { return null; }
        public Task<IIdentity> GetAsync(string key) { return Task.FromResult(Get(key)); }
        public void Remove(string key) { }
        public Task RemoveAsync(string key) { return Task.CompletedTask; }
        public void Set(string key, IIdentity value) { }
        public Task SetAsync(string key, IIdentity value) { return Task.CompletedTask; }
    }
}
