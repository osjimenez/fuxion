using Fuxion.Identity.Test.Dao;
using Fuxion.Identity.Test.Dao;
using Fuxion.Identity.Test.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using static Fuxion.Identity.Functions;
using static Fuxion.Identity.Test.Context;
using Xunit.Abstractions;
using Xunit;
using Fuxion.Test;
using SimpleInjector;
using Fuxion.Repositories;
using Fuxion.Identity.Test.Repositories;
using Fuxion.Factories;

namespace Fuxion.Identity.Test
{
    public class RolTest : BaseTest
    {
        public RolTest(ITestOutputHelper helper) : base(helper)
        {
            Container c = new Container();

            // TypeDiscriminators
            c.RegisterSingleton(new TypeDiscriminatorFactory().Transform(fac =>
            {
                fac.RegisterTree<BaseDao>(typeof(BaseDao).Assembly.DefinedTypes.ToArray());
                return fac;
            }));
            // IdentityManager
            c.Register<IPasswordProvider, PasswordProviderMock>();
            c.RegisterSingleton<ICurrentUserNameProvider>(new GenericCurrentUserNameProvider(() => Context.Rol.Identity.Root.UserName));
            c.RegisterSingleton<IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>>(new IdentityMemoryTestRepository());
            c.Register<IdentityManager>();
            //Factory.AddInjector(new InstanceInjector<IdentityManager>(new IdentityManager()));

            Factory.AddInjector(new SimpleInjectorFactoryInjector(c));
        }
        [Fact(DisplayName = "Rol - Can by instance")]
        public void CanByInstance()
        {
            new IdentityDao
            {
                Id = "oka",
                Name = "Oscar",
                //Permissions = new[] {
                //    new PermissionDao {
                //        Value =true,
                //        Function = Read.Id.ToString(),
                //        Scopes =new[] {
                //            new ScopeDao {
                //                Discriminator = Discriminator.Location.City.SanFrancisco,
                //                Propagation = ScopePropagation.ToMe }
                //        }
                //    },
                //}.ToList(),
                Groups = new[]
                {
                    new GroupDao
                    {
                        Id = "admins",
                        Name = "Admins",
                        Permissions = new[]
                        {
                            new PermissionDao {
                                Value =false,
                                Function =Edit.Id.ToString(),
                                Scopes =new[] {
                                    new ScopeDao {
                                        Discriminator = Discriminator.Location.State.California,
                                        Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions }
                                }
                            }
                        }
                    }
                }
            }.EnsureCan(Edit).Instance(Discriminator.Location.City.SanFrancisco);
        }
        [Fact(DisplayName = "Rol - Can by type")]
        public void CanByType()
        { 
            new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[] {
                    new PermissionDao {
                        Value =true,
                        Function = Admin.Id.ToString(),
                        Scopes =new[]{
                            new ScopeDao {
                                Discriminator = Discriminator.Category.Purchases,
                                Propagation = ScopePropagation.ToMe },
                            new ScopeDao {
                                Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<BaseDao>(),
                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions },
                        }
                    },
                    //new PermissionDao {
                    //    Value =false,
                    //    Function =Edit.Id.ToString(),
                    //    Scopes =new[] {
                    //        new ScopeDao {
                    //            Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<WordDocumentDao>(),
                    //            Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions }
                    //    }
                    //}
                }.ToList()
            }.EnsureCan(Read).Type<WordDocumentDao>();


            //Rol.Identity.Root.Can(Read).Instance(Discriminator.Location.City.SanFrancisco);

            //Assert.IsFalse(new Rol
            //{
            //    Id = "oka",
            //    Permissions = new[] {
            //        new Permission {
            //            Value = true,
            //            Function = Edit.Id.ToString(),
            //            Scopes = new[] {
            //                new Scope {
            //                    Discriminator = Locations.SanFrancisco,
            //                    Propagation = ScopePropagation.ToMe } } },
            //        new Permission {
            //            Value = false,
            //            Function = Edit.Id.ToString(),
            //            Scopes = new[] {
            //                new Scope {
            //                    Discriminator = Locations.California,
            //                    Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions } } }
            //    }.ToList()
            //}.IsFunctionAssigned(Edit, new[] { SanFranciscoDis }, (m, _) => Debug.WriteLine(m))
            //    , "Tengo:\r\n" +
            //        $" - Concedido el permiso para {nameof(Edit)} en {nameof(SanFrancisco)}\r\n" +
            //        $" - Denegado el permiso para {nameof(Edit)} en {nameof(California)} y sus hijos\r\n" +
            //        $"¿Debería poder {nameof(Edit)} en {nameof(SanFrancisco)}?\r\n" +
            //        " No");
        }
    }
}
