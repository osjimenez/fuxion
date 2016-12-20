using Fuxion.Identity.Test.Entity;
using Fuxion.Repositories;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Fuxion.Identity.Test.IdentityMemoryTestRepository;
using static Fuxion.Identity.Test.Context;
using Fuxion.Identity.Test;
using Fuxion.Data;
namespace Fuxion.Identity.DatabaseEFTest
{
    public class TestDatabaseInitializer : DropCreateDatabaseAlways<IdentityDatabaseEFTestRepository>
    {
        public override void InitializeDatabase(IdentityDatabaseEFTestRepository context)
        {
            try
            {
                context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction
                    , string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", context.Database.Connection.Database));
            }
            catch { }
            base.InitializeDatabase(context);
        }
        protected override void Seed(IdentityDatabaseEFTestRepository context)
        {
            context.Identity.AddRange(Identities);
            context.Album.AddRange(Albums);
            context.Song.AddRange(Songs);
            //context.Circle.AddRange(Circles);
            context.Group.AddRange(Groups);
            context.Document.AddRange(Documents);
            context.SaveChanges();
            base.Seed(context);
        }
    }
    public class IdentityDatabaseEFTestRepository : DbContext, IIdentityTestRepository, IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>
    {
        public IdentityDatabaseEFTestRepository() : base(nameof(IdentityDatabaseEFTestRepository))
        {

        }
        public void Initialize()
        {
            Database.SetInitializer(new TestDatabaseInitializer());
            Database.Initialize(true);
        }

        public DbSet<Test.Entity.Identity> Identity { get; set; }
        public DbSet<Album> Album { get; set; }
        public DbSet<Song> Song { get; set; }
        //public DbSet<Circle> Circle { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<Document> Document { get; set; }
        public IEnumerable<T> GetByType<T>()
        {
            if (typeof(T) == typeof(Album)) return (IEnumerable<T>)Album;
            if (typeof(T) == typeof(Song)) return (IEnumerable<T>)Song;
            //if (typeof(T) == typeof(Circle)) return (IEnumerable<T>)Circle;
            if (typeof(T) == typeof(Group)) return (IEnumerable<T>)Group;
            if (typeof(T) == typeof(Document)) return (IEnumerable<T>)Document;
            throw new KeyNotFoundException();
        }
        IEnumerable<Album> IIdentityTestRepository.Album { get { return Album.Include(a => a.Songs); } }
        IEnumerable<Song> IIdentityTestRepository.Song { get { return Song; } }
        //IEnumerable<Circle> IIdentityTestRepository.Circle { get { return Circle; } }
        IEnumerable<Group> IIdentityTestRepository.Group { get { return Group; } }
        IEnumerable<Document> IIdentityTestRepository.Document { get { return Document; } }

        public IIdentity Find(string key)
        {
            
            var ide = Identity.FirstOrDefault(i => i.UserName == key);
            if (ide == null) return null;
            var groupsRef = Entry(ide).Collection(i => i.Groups);
            if (!groupsRef.IsLoaded) groupsRef.Load();
            foreach (var gro in ide.Groups) LoadGroups(gro);

            LoadPermissions(ide);

            return ide;
        }
        private void LoadGroups(Rol rol)
        {
            var @ref = Entry(rol).Collection(g => g.Groups);
            if (!@ref.IsLoaded) @ref.Load();
            LoadPermissions(rol);
            foreach (var g in rol.Groups) LoadGroups(g);
        }
        private void LoadPermissions(Rol rol)
        {
            var persRef = Entry(rol).Collection(r => r.Permissions);
            if (!persRef.IsLoaded) persRef.Load();
            foreach (var per in rol.Permissions)
            {
                var scosRef = Entry(per).Collection(p => p.Scopes);
                if (!scosRef.IsLoaded) scosRef.Load();
                foreach (var sco in per.Scopes)
                {
                    var disRef = Entry(sco).Reference(s => s.Discriminator);
                    if (!disRef.IsLoaded) disRef.Load();
                    LoadDiscriminator(sco.Discriminator);
                }
            }
        }
        private void LoadDiscriminator(Discriminator discriminator)
        {
            if (discriminator is Country)
            {
                var statesRef = Entry((Country)discriminator).Collection(c => c.States);
                if (!statesRef.IsLoaded)
                {
                    statesRef.Load();
                    foreach (var sta in ((Country)discriminator).States)
                        LoadDiscriminator(sta);
                }
            }
            else if (discriminator is State)
            {
                var citiesRef = Entry((State)discriminator).Collection(s => s.Cities);
                if (!citiesRef.IsLoaded)
                {
                    citiesRef.Load();
                    foreach (var cit in ((State)discriminator).Cities)
                        LoadDiscriminator(cit);
                }
                var countryRef = Entry((State)discriminator).Reference(s => s.Country);
                if (!countryRef.IsLoaded)
                {
                    countryRef.Load();
                    LoadDiscriminator(((State)discriminator).Country);
                }
            }
            else if (discriminator is City)
            {
                var stateRef = Entry((City)discriminator).Reference(c => c.State);
                if (!stateRef.IsLoaded)
                {
                    stateRef.Load();
                    LoadDiscriminator(((City)discriminator).State);
                }
            }
        }
        public async Task<IIdentity> FindAsync(string key) { return await Identity.SingleOrDefaultAsync(i => i.UserName == key).ConfigureAwait(false); }
        public bool Exist(string key) { return Find(key) != null; }
        public async Task<bool> ExistAsync(string key) { return await FindAsync(key).ConfigureAwait(false) != null; }
        public IIdentity Get(string key) {
            var res = Find(key);
            if (res == null) throw new KeyNotFoundException($"Key '{key}' was not found in database");
            return res;
        }
        public async Task<IIdentity> GetAsync(string key) {
            var res = await FindAsync(key).ConfigureAwait(false);
            if (res == null) throw new KeyNotFoundException($"Key '{key}' was not found in database");
            return res;
        }
        public void Remove(string key) { throw new NotImplementedException(); }
        public Task RemoveAsync(string key) { throw new NotImplementedException(); }
        public new void Set(string key, IIdentity value) { throw new NotImplementedException(); }
        public Task SetAsync(string key, IIdentity value) { throw new NotImplementedException(); }
    }
}
