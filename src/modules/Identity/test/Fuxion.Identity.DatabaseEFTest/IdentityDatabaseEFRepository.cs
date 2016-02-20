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
namespace Fuxion.Identity.DatabaseTest
{
    class IdentityDatabaseEFRepository : DbContext, IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>
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
        //public DbSet<Test.Entity.Identity> Identity { get; set; }
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
            return Identity.Include(i=>i.Permissions).SingleOrDefault(i => i.UserName == key);
        }

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
