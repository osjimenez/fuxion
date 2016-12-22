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
        //IEnumerable<Circle> Circle { get; }
        IEnumerable<Group> Group { get; }
        IEnumerable<Document> Document { get; }
        IEnumerable<T> GetByType<T>();
    }
    public class IdentityMemoryTestRepository : IIdentityTestRepository, IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>
    {
        public IEnumerable<Album> Album { get { return Context.File.Package.Album.GetAll(); } }
        public IEnumerable<Song> Song { get { return Context.File.Media.Song.GetAll(); } }
        //public IEnumerable<Circle> Circle { get { return Circles; } }
        public IEnumerable<Group> Group { get { return Context.Rol.Group.GetAll(); } }
        public IEnumerable<Document> Document { get { return Context.File.Document.GetAll(); } }

        public IEnumerable<T> GetByType<T>()
        {
            if (typeof(T) == typeof(Album))  return (IEnumerable<T>)Album;
            if (typeof(T) == typeof(Song))   return (IEnumerable<T>)Song;
            //if (typeof(T) == typeof(Circle)) return (IEnumerable<T>)Circle;
            if (typeof(T) == typeof(Group))  return (IEnumerable<T>)Group;
            throw new KeyNotFoundException();
        }

        public bool Exist(string key) { return false; }
        public Task<bool> ExistAsync(string key) { return Task.FromResult(Exist(key)); }
        public IIdentity Find(string key) { return Context.Rol.Identity.GetAll().FirstOrDefault(i => i.UserName == key); }
        public Task<IIdentity> FindAsync(string key) { return Task.FromResult(Find(key)); }
        public IIdentity Get(string key) { return null; }
        public Task<IIdentity> GetAsync(string key) { return Task.FromResult(Get(key)); }
        public void Remove(string key) { }
        public Task RemoveAsync(string key) { return Task.CompletedTask; }
        public void Set(string key, IIdentity value) { }
        public Task SetAsync(string key, IIdentity value) { return Task.CompletedTask; }
    }
}
