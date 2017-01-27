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
            //new IdentityDao
            //{
            //    Id = "test",
            //    Name = "Test",
            //    Groups = new[]
            //    {
            //        new GroupDao
            //        {
            //            Id = "admins",
            //            Name = "Admins",
            //            Permissions = new[]
            //            {
            //                new PermissionDao {
            //                    Value = true,
            //                    Function =Edit.Id.ToString(),
            //                    Scopes =new[] {
            //                        new ScopeDao {
            //                            Discriminator = Discriminator.Location.State.California,
            //                            Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}.EnsureCan(Edit).Instance(Discriminator.Location.City.SanFrancisco);
            new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Groups = new[]
                {
                    new GroupDao
                    {
                        Id = "admins",
                        Name = "Admins",
                        Permissions = new[]
                        {
                            new PermissionDao {
                                Value = true,
                                Function =Edit.Id.ToString(),
                                Scopes =new[] {
                                    new ScopeDao {
                                        Discriminator = Discriminator.Location.State.California,
                                        Propagation = ScopePropagation.ToMe | ScopePropagation.ToExclusions }
                                }
                            }
                        }
                    }
                }
            }.EnsureCan(Edit).Instance(Discriminator.Location.Country.Usa);
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
        }
        [Fact(DisplayName = "Rol - Cannot by type")]
        public void CannotByType()
        { 
            Assert.False(new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[] {
                    new PermissionDao {
                        Value = true,
                        Function = Edit.Id.ToString(),
                        Scopes = new[] {
                            new ScopeDao {
                                Discriminator = Discriminator.Location.City.SanFrancisco,
                                Propagation = ScopePropagation.ToMe } } },
                    new PermissionDao {
                        Value = false,
                        Function = Edit.Id.ToString(),
                        Scopes = new[] {
                            new ScopeDao {
                                Discriminator = Discriminator.Location.State.California,
                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions } } }
                }.ToList()
            }.Can(Edit).Instance(Discriminator.Location.City.SanFrancisco)
            //}.IsFunctionAssigned(Edit, new[] { Discriminator.Location.City.SanFrancisco })
                , "Tengo:\r\n" +
                    $" - Concedido el permiso para {nameof(Edit)} en {nameof(Discriminator.Location.City.SanFrancisco)}\r\n" +
                    $" - Denegado el permiso para {nameof(Edit)} en {nameof(Discriminator.Location.State.California)} y sus hijos\r\n" +
                    $"¿Debería poder {nameof(Edit)} en {nameof(Discriminator.Location.City.SanFrancisco)}?\r\n" +
                    " No");
        }
    }
}
