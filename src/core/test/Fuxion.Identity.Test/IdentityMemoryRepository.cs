using Fuxion.Identity.Test.Dao;
using Fuxion.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Fuxion.Identity.Test
{
	public interface IIdentityTestRepository
	{
		IEnumerable<AlbumDao> Album { get; }
		IEnumerable<SongDao> Song { get; }
		//IEnumerable<Circle> Circle { get; }
		IEnumerable<GroupDao> Group { get; }
		IEnumerable<DocumentDao> Document { get; }
		IEnumerable<T> GetByType<T>();
	}
	public class IdentityMemoryTestRepository : IIdentityTestRepository, IKeyValueRepository<string, IIdentity>
	{
		public IEnumerable<AlbumDao> Album => Albums.All;
		public IEnumerable<SongDao> Song => Songs.All;
		//public IEnumerable<Circle> Circle { get { return Circles; } }
		public IEnumerable<GroupDao> Group => Groups.All;
		public IEnumerable<DocumentDao> Document => Documents.All;

		public IEnumerable<T> GetByType<T>()
		{
			if (typeof(T) == typeof(AlbumDao)) return (IEnumerable<T>)Album;
			if (typeof(T) == typeof(SongDao)) return (IEnumerable<T>)Song;
			//if (typeof(T) == typeof(Circle)) return (IEnumerable<T>)Circle;
			if (typeof(T) == typeof(GroupDao)) return (IEnumerable<T>)Group;
			throw new KeyNotFoundException();
		}

		public bool Exist(string key) => false;
		public Task<bool> ExistAsync(string key) => Task.FromResult(Exist(key));
		public IIdentity Find(string key) => Identities.All.FirstOrDefault(i => i.UserName == key);
		public Task<IIdentity> FindAsync(string key) => Task.FromResult(Find(key));
		public IIdentity Get(string key) => default!;
		public Task<IIdentity> GetAsync(string key) => Task.FromResult(Get(key));
		public void Remove(string key) { }
		public Task RemoveAsync(string key) => Task.CompletedTask;
		public void Set(string key, IIdentity value) { }
		public Task SetAsync(string key, IIdentity value) => Task.CompletedTask;
	}
}
