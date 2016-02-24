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
using static Fuxion.Identity.Test.StaticContext;
using Fuxion.Identity.Test;
using Fuxion.Data;
namespace Fuxion.Identity.DatabaseEFTest
{
    public class IdentityDatabaseEFTestRepository : DbContext, IIdentityTestRepository, IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>
    {
        //public IdentityDatabaseEFTestRepository()
        //{
        //    Database.SetInitializer(new DropCreateDatabaseAlways<IdentityDatabaseEFTestRepository>());
        //}
        public void InitializeData()
        {
            this.ClearData();
            Identity.AddRange(Identities);
            Order.AddRange(SellOrders);
            Invoice.AddRange(Invoices);
            SaveChanges();
        }

        public DbSet<Test.Entity.Identity> Identity { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<Invoice> Invoice { get; set; }
        IEnumerable<Order> IIdentityTestRepository.Order { get { return Order; } }
        IEnumerable<Invoice> IIdentityTestRepository.Invoice { get { return Invoice; } }

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

        public async Task<IIdentity> FindAsync(string key) { return await Identity.SingleOrDefaultAsync(i => i.UserName == key); }
        public bool Exist(string key) { return Identity.Any(i => i.UserName == key); }
        public async Task<bool> ExistAsync(string key) { return await Identity.AnyAsync(i => i.UserName == key); }
        public IIdentity Get(string key) { return Identity.Single(i => i.UserName == key); }
        public async Task<IIdentity> GetAsync(string key) { return await Identity.SingleAsync(i => i.UserName == key); }
        public void Remove(string key) { throw new NotImplementedException(); }
        public Task RemoveAsync(string key) { throw new NotImplementedException(); }
        public new void Set(string key, IIdentity value) { throw new NotImplementedException(); }
        public Task SetAsync(string key, IIdentity value) { throw new NotImplementedException(); }
    }
}
