using Fuxion.Identity.Test.Entity;
using Fuxion.Repositories;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Fuxion.Identity.Test.IdentityMemoryRepository;
namespace Fuxion.Identity.DatabaseEFTest
{
    public class IdentityDatabaseEFRepository : DbContext, IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>
    {
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    //optionsBuilder.UseInMemoryDatabase();
        //    optionsBuilder.UseSqlServer(@"Data Source=.\sqlexpress;Initial Catalog=FuxionIdentity;Integrated Security=True");
        //    base.OnConfiguring(optionsBuilder);
        //}
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<RolGroup>()
        //        .HasKey(t => new { t.RolId, t.GroupId });
        //    //modelBuilder.Entity<PostTag>()
        //    //    .HasOne(pt => pt.Post)
        //    //    .WithMany(p => p.PostTags)
        //    //    .HasForeignKey(pt => pt.PostId);

        //    //modelBuilder.Entity<PostTag>()
        //    //    .HasOne(pt => pt.Tag)
        //    //    .WithMany(t => t.PostTags)
        //    //    .HasForeignKey(pt => pt.TagId);
        //    modelBuilder.Entity<RolGroup>()
        //        .HasOne(rg => rg.Rol)
        //        .WithMany(rol => rol.RolGroups)
        //        .ForeignKey(pt => pt.RolId);

        //        //.HasForeignKey(pt => pt);

        //    modelBuilder.Entity<RolGroup>()
        //        .HasOne(pt => pt.Group)
        //        .WithMany(t => t.RolGroups)
        //        .ForeignKey(pt => pt.RolId);
        //}
        public DbSet<Test.Entity.Identity> Identity { get; set; }
        public DbSet<Order> Order { get; set; }
        //public DbSet<Rol> Rols { get; set; }
        //public DbSet<Group> Groups { get; set; }
        //public DbSet<Demo1> Demo1 { get; set; }
        //public DbSet<Demo2> Demo2 { get; set; }
        public bool Exist(string key)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistAsync(string key)
        {
            throw new NotImplementedException();
        }

        public IIdentity Find(string key)
        {
            
            var ide = Identity.FirstOrDefault(i => i.UserName == key);
            if (ide == null) return null;
            var groupsRef = Entry(ide).Collection(i => i.Groups);
            if (!groupsRef.IsLoaded) groupsRef.Load();
            foreach (var gro in ide.Groups) LoadGroupGroups(gro);

            LoadRolPermissions(ide);

            return ide;
        }
        private void LoadGroupGroups(Group gro)
        {
            var @ref = Entry(gro).Collection(g => g.Groups);
            if (!@ref.IsLoaded) @ref.Load();
            LoadRolPermissions(gro);
            foreach (var g in gro.Groups) LoadGroupGroups(g);
        }
        private void LoadRolPermissions(Rol rol)
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
            // Locations (Country, State, City)
            // Departments
            // Categories
            // Tags
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
            else if (discriminator is Department)
            {
                var childrenRef = Entry((Department)discriminator).Collection(d => d.Children);
                if (!childrenRef.IsLoaded)
                {
                    childrenRef.Load();
                    foreach (var dep in ((Department)discriminator).Children)
                        LoadDiscriminator(dep);
                }
                var parentRef = Entry((Department)discriminator).Reference(d => d.Parent);
                if (!parentRef.IsLoaded)
                {
                    parentRef.Load();
                    LoadDiscriminator(((Department)discriminator).Parent);
                }
            }
        }
        //private void LoadDiscriminatorInclusions(Discriminator dom)
        //{
        //    var childrenRef = Entry(dom).Collection(d => d.Inclusions);
        //    if (!childrenRef.IsLoaded) childrenRef.Load();
        //    if(dom.Inclusions != null) foreach (var d in dom.Inclusions) LoadDiscriminatorInclusions(d);
        //}
        //private void LoadDiscriminatorExclusions(Discriminator dom)
        //{
        //    var parentRef = Entry(dom).Collection(d => d.Exclusions);
        //    if (!parentRef.IsLoaded) parentRef.Load();
        //    if (dom.Exclusions != null) foreach(var d in dom.Exclusions) LoadDiscriminatorExclusions(d);
        //}

        public async Task<IIdentity> FindAsync(string key)
        {
            return await Identity.SingleOrDefaultAsync(i => i.UserName == key);
        }

        public IIdentity Get(string key)
        {
            return Identity.Single(i => i.UserName == key);
        }

        public async Task<IIdentity> GetAsync(string key)
        {
            return await Identity.SingleAsync(i => i.UserName == key);
        }

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string key)
        {
            throw new NotImplementedException();
        }

        public new void Set(string key, IIdentity value)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(string key, IIdentity value)
        {
            throw new NotImplementedException();
        }
    }
}
