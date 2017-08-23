//using Fuxion.Identity.Test.Dao;
//using Fuxion.Identity.Test.Mocks;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Diagnostics;
//using static Fuxion.Identity.Functions;
//using static Fuxion.Identity.Test.Context;
//using Xunit.Abstractions;
//using Xunit;
//using Fuxion.Test;
//using SimpleInjector;
//using Fuxion.Repositories;
//using Fuxion.Identity.Test.Repositories;
//using Fuxion.Factories;
//using Fuxion.Identity.Test.Helpers;

//namespace Fuxion.Identity.Test
//{
//    public class RolTest : BaseTest
//    {
//        public RolTest(ITestOutputHelper helper) : base(helper)
//        {
//            Container c = new Container();

//            // TypeDiscriminators
//            c.RegisterSingleton(new TypeDiscriminatorFactory().Transform(fac =>
//            {
//                fac.RegisterTree<BaseDao>(typeof(BaseDao).Assembly.DefinedTypes.ToArray());
//                return fac;
//            }));
//            // IdentityManager
//            c.Register<IPasswordProvider, PasswordProviderMock>();
//            c.RegisterSingleton<ICurrentUserNameProvider>(new GenericCurrentUserNameProvider(() => Context.Rol.Identity.Root.UserName));
//            c.RegisterSingleton<IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>>(new IdentityMemoryTestRepository());
//            c.Register<IdentityManager>();
//            //Factory.AddInjector(new InstanceInjector<IdentityManager>(new IdentityManager()));

//            Factory.AddInjector(new SimpleInjectorFactoryInjector(c));
//        }
//        [Fact(DisplayName = "Rol - Cannot by function")]
//        public void CannotByFunction()
//        {
//            var ide = new IdentityDao
//            {
//                Id = "test",
//                Name = "Test",
//                Permissions = new[]
//                {
//                    new PermissionDao {
//                        Value = true,
//                        Function = Edit.Id.ToString(),
//                        Scopes = new[] {
//                            new ScopeDao {
//                                Discriminator = Discriminators.Category.Purchases,
//                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToExclusions }
//                        }
//                    }
//                },
//            };
//            Assert.False(ide.Can(Delete).Instance(Discriminators.Category.Purchases),
//                "Tengo:\r\n" +
//                    $" - Concedido el permiso para {nameof(Edit)} en {nameof(Discriminators.Category.Purchases)}\r\n" +
//                    $"¿Debería poder {nameof(Delete)} en {nameof(Discriminators.Category.Purchases)}?\r\n" +
//                    " No");
//            Assert.True(ide.Can(Read).Instance(Discriminators.Category.Purchases),
//                "Tengo:\r\n" +
//                    $" - Concedido el permiso para {nameof(Edit)} en {nameof(Discriminators.Category.Purchases)}\r\n" +
//                    $"¿Debería poder {nameof(Read)} en {nameof(Discriminators.Category.Purchases)}?\r\n" +
//                    " Si");

//            Assert.False(ide.Can(Delete).Type<WordDocumentDao>(),
//                "Tengo:\r\n" +
//                    $" - Concedido el permiso para {nameof(Edit)} en {nameof(Discriminators.Category.Purchases)}\r\n" +
//                    $"¿Debería poder {nameof(Delete)} en {nameof(Discriminators.Category.Purchases)}?\r\n" +
//                    " No");
//            Assert.True(ide.Can(Read).Type<WordDocumentDao>(),
//                "Tengo:\r\n" +
//                    $" - Concedido el permiso para {nameof(Edit)} en {nameof(Discriminators.Category.Purchases)}\r\n" +
//                    $"¿Debería poder {nameof(Read)} en {nameof(Discriminators.Category.Purchases)}?\r\n" +
//                    " Si");
//        }
//        [Fact(DisplayName = "Rol - Cannot for different discriminator")]
//        public void CannotForDifferent()
//        {
//            var ide = new IdentityDao
//            {
//                Id = "test",
//                Name = "Test",
//                Groups = new[]
//                {
//                    new GroupDao
//                    {
//                        Id = "admins",
//                        Name = "Admins",
//                        Permissions = new[]
//                        {
//                            new PermissionDao {
//                                Value = true,
//                                Function = Edit.Id.ToString(),
//                                Scopes =new[] {
//                                    new ScopeDao {
//                                        Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<CategoryDao>(),
//                                        Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions }
//                                }
//                            }
//                        }
//                    }
//                }
//            };
//            //Assert.True(ide.Can(Read).Instance(Discriminators.Cate.City.Buffalo),
//            // "Tengo:\r\n" +
//            //    $" - Concedido el permiso para {nameof(Edit)} el tipo {nameof(CategoryDao)} y derivados\r\n" +
//            //    $"¿Debería poder {nameof(Read)} en {nameof(Discriminators.Location.City.Buffalo)}?\r\n" +
//            //    " No");
//            Assert.False(ide.Can(Read).Instance(Discriminators.Location.City.Buffalo),
//                 "Tengo:\r\n" +
//                    $" - Concedido el permiso para {nameof(Edit)} el tipo {nameof(CategoryDao)} y derivados\r\n" +
//                    $"¿Debería poder {nameof(Read)} en {nameof(Discriminators.Location.City.Buffalo)}?\r\n" +
//                    " No");
//        }
//        [Fact(DisplayName = "Rol - Cannot for different discriminator2")]
//        public void CannotForDifferent2()
//        {
//            var ide = new IdentityDao
//            {
//                Id = "test",
//                Name = "Test",
//                Groups = new[]
//                {
//                    new GroupDao
//                    {
//                        Id = "admins",
//                        Name = "Admins",
//                        Permissions = new[]
//                        {
//                            new PermissionDao {
//                                Value = true,
//                                Function = Edit.Id.ToString(),
//                                Scopes =new[] {
//                                    new ScopeDao {
//                                        Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<CategoryDao>(),
//                                        Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions }
//                                }
//                            },
//                            new PermissionDao {
//                                Value = false,
//                                Function = Read.Id.ToString(),
//                                Scopes =new[] {
//                                    new ScopeDao {
//                                        Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<CategoryDao>(),
//                                        Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions }
//                                }
//                            },
//                        }
//                    }
//                }
//            };
//            Assert.False(ide.Can(Read).Instance(Discriminators.Category.Purchases),
//                 "Tengo:\r\n" +
//                    $" - Concedido el permiso para {nameof(Edit)} el tipo {nameof(CategoryDao)} y derivados\r\n" +
//                    $" - Denegado el permiso para {nameof(Read)} el tipo {nameof(CategoryDao)} y derivados\r\n" +
//                    $"¿Debería poder {nameof(Read)} en {nameof(Discriminators.Category.Purchases)}?\r\n" +
//                    " No");
//        }
//        [Fact(DisplayName = "Rol - Cannot admin something")]
//        public void CannotAdminSomething()
//        {
//            var ide = new IdentityDao
//            {
//                Id = "test",
//                Name = "Test",
//                Groups = new[]
//                {
//                    new GroupDao
//                    {
//                        Id = "admins",
//                        Name = "Admins",
//                        Permissions = new[]
//                        {
//                            new PermissionDao {
//                                Value = true,
//                                Function = Edit.Id.ToString(),
//                                Scopes =new[] {
//                                    new ScopeDao {
//                                        Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<CategoryDao>(),
//                                        Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions }
//                                }
//                            },
//                            new PermissionDao {
//                                Value = false,
//                                Function = Read.Id.ToString(),
//                                //Scopes =new[] {
//                                //    new ScopeDao {
//                                //        Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<CategoryDao>(),
//                                //        Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions }
//                                //}
//                            },
//                        }
//                    }
//                }
//            };
//            Assert.False(ide.Can(Admin).Something(),
//                 "Tengo:\r\n" +
//                    $" - Concedido el permiso para {nameof(Edit)} el tipo {nameof(CategoryDao)} y derivados\r\n" +
//                    $" - Denegado el permiso para {nameof(Read)} cualquier cosa\r\n" +
//                    $"¿Debería poder {nameof(Admin)} alguna cosa?\r\n" +
//                    " No");
//        }
//        [Fact(DisplayName = "Rol - Can by instance multiple properties")]
//        public void CanByInstance2()
//        {
//            var ide = new IdentityDao
//            {
//                Id = "test",
//                Name = "Test",
//                Groups = new[]
//                {
//                    new GroupDao
//                    {
//                        Id = "admins",
//                        Name = "Admins",
//                        Permissions = new[]
//                        {
//                            new PermissionDao {
//                                Value = true,
//                                Function = Edit.Id.ToString(),
//                                Scopes =new[] {
//                                    new ScopeDao {
//                                        Discriminator = Discriminators.Category.Purchases,
//                                        Propagation = ScopePropagation.ToMe | ScopePropagation.ToExclusions }
//                                }
//                            }
//                        }
//                    }
//                }
//            }.EnsureCan(Edit).Instance(Discriminators.Category.Purchases);
//        }
//        [Fact(DisplayName = "Rol - Can by instance")]
//        public void CanByInstance()
//        {
//            new IdentityDao
//            {
//                Id = "test",
//                Name = "Test",
//                Groups = new[]
//                {
//                    new GroupDao
//                    {
//                        Id = "admins",
//                        Name = "Admins",
//                        Permissions = new[]
//                        {
//                            new PermissionDao {
//                                Value = true,
//                                Function =Edit.Id.ToString(),
//                                Scopes =new[] {
//                                    new ScopeDao {
//                                        Discriminator = Discriminators.Location.State.California,
//                                        Propagation = ScopePropagation.ToMe | ScopePropagation.ToExclusions }
//                                }
//                            }
//                        }
//                    }
//                }
//            }.EnsureCan(Edit).AllLocations(Discriminators.Location.Country.Usa);
//        }
//        [Fact(DisplayName = "Rol - Cannot by instance")]
//        public void CannotByInstance()
//        {
//            Assert.False(
//                new IdentityDao
//                {
//                    Id = "test",
//                    Name = "Test",
//                    Groups = new[]
//                    {
//                        new GroupDao
//                        {
//                            Id = "admins",
//                            Name = "Admins",
//                            Permissions = new[]
//                            {
//                                new PermissionDao {
//                                    Value = true,
//                                    Function =Edit.Id.ToString(),
//                                    Scopes =new[] {
//                                        new ScopeDao {
//                                            Discriminator = Discriminators.Category.Purchases,
//                                            Propagation = ScopePropagation.ToMe }
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }.Can(Edit).Instance(File.Document.Word.Word1));
//        }
//        [Fact(DisplayName = "Rol - Can by type, same discriminator type")]
//        public void CanByType()
//        {
//            var ide = new IdentityDao
//            {
//                Id = "test",
//                Name = "Test",
//                Permissions = new[] {
//                    new PermissionDao {
//                        Value =true,
//                        Function = Admin.Id.ToString(),
//                        Scopes =new[]{
//                            new ScopeDao {
//                                Discriminator = Discriminators.Category.Purchases,
//                                Propagation = ScopePropagation.ToMe },
//                            new ScopeDao {
//                                Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<BaseDao>(),
//                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions },
//                        }
//                    },
//                }.ToList()
//            };
//            Assert.True(ide.Can(Read).Type<WordDocumentDao>(),
//                 "Tengo:\r\n" +
//                    $" - Concedido el permiso para {nameof(Admin)} la categoria {Discriminators.Category.Purchases} del tipo {nameof(BaseDao)} y derivados\r\n" +
//                    $"¿Debería poder {nameof(Read)} en {nameof(WordDocumentDao)}?\r\n" +
//                    " Si");
//            Assert.True(ide.Can(Admin).Something(),
//                 "Tengo:\r\n" +
//                    $" - Concedido el permiso para {nameof(Admin)} la categoria {Discriminators.Category.Purchases} del tipo {nameof(BaseDao)} y derivados\r\n" +
//                    $"¿Debería poder {nameof(Admin)} alguna cosa?\r\n" +
//                    " Si");
//        }
//        [Fact(DisplayName = "Rol - Can by type, different discriminator type")]
//        public void CanByType2()
//        {
//            var ide = new IdentityDao
//            {
//                Id = "test",
//                Name = "Test",
//                Permissions = new[] {
//                    new PermissionDao {
//                        Value =true,
//                        Function = Admin.Id.ToString(),
//                        Scopes =new[]{
//                            new ScopeDao {
//                                Discriminator = Discriminators.Category.Purchases,
//                                Propagation = ScopePropagation.ToMe },
//                        }
//                    },
//                }.ToList()
//            };
//            Assert.True(ide.Can(Read).Type<WordDocumentDao>(), "\r\n" +
//                $" - Granted permission for '{nameof(Admin)}' anything of category '{nameof(Discriminators.Category.Purchases)}'\r\n" +
//                $"Should i be able to '{nameof(Read)}' objects of type '{nameof(WordDocumentDao)}'?\r\n" +
//                $"YES");
//        }
//        [Fact(DisplayName = "Rol - Can by instance, mix root permission with others")]
//        public void CanByType3()
//        {
//            var ide = new IdentityDao
//            {
//                Id = "test",
//                Name = "Test",
//                Permissions = new[] {
//                    new PermissionDao {
//                        Value = true,
//                        Function = Admin.Id.ToString(),
//                    },
//                    new PermissionDao {
//                        Value =true,
//                        Function = Admin.Id.ToString(),
//                        Scopes =new[]{
//                            new ScopeDao {
//                                Discriminator = Discriminators.Category.Purchases,
//                                Propagation = ScopePropagation.ToMe },
//                        }
//                    },
//                }.ToList()
//            };
//            Assert.True(ide.EnsureCan(Read).Instance(File.Document.Word.Word1),
//                 "Tengo:\r\n" +
//                    $" - Concedido el permiso para {nameof(Admin)} en {nameof(Discriminators.Category.Purchases)}\r\n" +
//                    $"¿Debería poder {nameof(Read)} en {nameof(File.Document.Word.Word1)}?\r\n" +
//                    " Si");
//            Assert.True(ide.EnsureCan(Read).Something(),
//                 "Tengo:\r\n" +
//                    $" - Concedido el permiso para {nameof(Admin)} en {nameof(Discriminators.Category.Purchases)}\r\n" +
//                    $"¿Debería poder {nameof(Read)} alguna cosa?\r\n" +
//                    " Si");
//        }
//        [Fact(DisplayName = "Rol - Cannot by type")]
//        public void CannotByType()
//        {
//            var ide = new IdentityDao
//            {
//                Id = "test",
//                Name = "Test",
//                Permissions = new[] {
//                    new PermissionDao {
//                        Value = true,
//                        Function = Edit.Id.ToString(),
//                        Scopes = new[] {
//                            new ScopeDao {
//                                Discriminator = Discriminators.Location.City.SanFrancisco,
//                                Propagation = ScopePropagation.ToMe } } },
//                    new PermissionDao {
//                        Value = false,
//                        Function = Edit.Id.ToString()   ,
//                        Scopes = new[] {
//                            new ScopeDao {
//                                Discriminator = Discriminators.Location.State.California,
//                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions } } }
//                }.ToList()
//            };
//            Assert.False(ide.IsRoot(),
//                "Tengo:\r\n" +
//                    $" - Concedido el permiso para {nameof(Edit)} en {nameof(Discriminators.Location.City.SanFrancisco)}\r\n" +
//                    $" - Denegado el permiso para {nameof(Edit)} en {nameof(Discriminators.Location.State.California)} y sus hijos\r\n" +
//                    $"¿Debería ser root?\r\n" +
//                    " No");
//            Assert.False(ide.Can(Edit).Instance(Discriminators.Location.City.SanFrancisco)
//                , "Tengo:\r\n" +
//                    $" - Concedido el permiso para {nameof(Edit)} en {nameof(Discriminators.Location.City.SanFrancisco)}\r\n" +
//                    $" - Denegado el permiso para {nameof(Edit)} en {nameof(Discriminators.Location.State.California)} y sus hijos\r\n" +
//                    $"¿Debería poder {nameof(Edit)} en {nameof(Discriminators.Location.City.SanFrancisco)}?\r\n" +
//                    " No");
//        }
//        [Fact(DisplayName = "Rol - No permissions")]
//        public void NoPermissions()
//        {
//            var ide = new IdentityDao
//            {
//                Id = "test",
//                Name = "Test",
//            };
//            Assert.False(ide.IsRoot(),
//                "Tengo:\r\n" +
//                    $" - Ningún permiso\r\n" +
//                    $"¿Debería ser root?\r\n" +
//                    " No");
//            Assert.False(ide.Can(Read).Something(),
//                "Tengo:\r\n" +
//                    $" - Ningún permiso\r\n" +
//                    $"¿Debería poder '{nameof(Read)}' alguna cosa?\r\n" +
//                    " No");
//            Assert.False(ide.Can(Edit).Instance(Discriminators.Location.City.SanFrancisco)
//                , "Tengo:\r\n" +
//                    $" - Ningun permiso\r\n" +
//                    $"¿Debería poder {nameof(Edit)} en {nameof(Discriminators.Location.City.SanFrancisco)}?\r\n" +
//                    " No");
//        }
//        [Fact(DisplayName = "Rol - Root permission")]
//        public void RootPermission()
//        {
//            var ide = new IdentityDao
//            {
//                Id = "test",
//                Name = "Test",
//                Permissions = new[] {
//                    new PermissionDao {
//                        Value = true,
//                        Function = Admin.Id.ToString(),
//                    }
//                }.ToList()
//            };
//            Assert.True(ide.IsRoot(),
//                "Tengo:\r\n" +
//                    $" - Permiso ROOT\r\n" +
//                    $"¿Debería ser root?\r\n" +
//                    " Si");
//            Assert.True(ide.Can(Admin).Anything(),
//                "Tengo:\r\n" +
//                    $" - Permiso ROOT\r\n" +
//                    $"¿Debería poder '{nameof(Admin)}' cualquier cosa?\r\n" +
//                    " Si");
//            Assert.True(ide.Can(Admin).Something(),
//                "Tengo:\r\n" +
//                    $" - Permiso ROOT\r\n" +
//                    $"¿Debería poder '{nameof(Admin)}' alguna cosa?\r\n" +
//                    " Si");
//            Assert.True(ide.Can(Edit).Instance(File.Document.Word.Word1),
//                "Tengo:\r\n" +
//                    $" - Permiso ROOT\r\n" +
//                    $"¿Debería poder '{nameof(Edit)}' en '{nameof(File.Document.Word.Word1)}'?\r\n" +
//                    " Si");
//            Assert.True(ide.Can(Edit).Type<CityDao>(),
//                "Tengo:\r\n" +
//                    $" - Permiso ROOT\r\n" +
//                    $"¿Debería poder '{nameof(Edit)}' las '{nameof(CityDao)}'?\r\n" +
//                    " Si");
//        }
//        [Fact(DisplayName = "Rol - Only read permission")]
//        public void OnlyReadPermission()
//        {
//            var ide = new IdentityDao
//            {
//                Id = "test",
//                Name = "Test",
//                Permissions = new[] {
//                    new PermissionDao {
//                        Value = true,
//                        Function = Read.Id.ToString(),
//                    }
//                }.ToList()
//            };
//            Assert.True(ide.Can(Read).Type<CityDao>(),
//                "Tengo:\r\n" +
//                    $" - Permiso ONLY READ\r\n" +
//                    $"¿Debería poder '{nameof(Read)}' las '{nameof(CityDao)}'?\r\n" +
//                    " Si");
//            Assert.False(ide.Can(Create).Type<CityDao>(),
//                "Tengo:\r\n" +
//                    $" - Permiso ONLY READ\r\n" +
//                    $"¿Debería poder '{nameof(Create)}' las '{nameof(CityDao)}'?\r\n" +
//                    " No");
//        }
//        [Fact(DisplayName = "Rol - Empty discriminator")]
//        public void EmptyDiscriminator()
//        {
//            var ide = new IdentityDao
//            {
//                Id = "test",
//                Name = "Test",
//                Permissions = new[] {
//                    new PermissionDao {
//                        Value = true,
//                        Function = Admin.Id.ToString(),
//                        Scopes = new[] {
//                            new ScopeDao
//                            {
//                                Discriminator = Discriminators.Category.Purchases,
//                                Propagation = ScopePropagation.ToMe| ScopePropagation.ToInclusions
//                            }
//                        }
//                    }
//                }.ToList()
//            };
//            Assert.False(ide.Can(Create).Instance(File.Document.Word.Word1),
//               "Tengo:\r\n" +
//                   $" - Permiso ADMIN en la categoria 'Purchases' y sus subcategorias\r\n" +
//                   $"¿Debería poder '{nameof(Create)}' una instancia de documento Word sin categoria?\r\n" +
//                   " No");
//            Assert.False(ide.Can(Create).ByAll(
//                    Factory.Get<TypeDiscriminatorFactory>().FromType<WordDocumentDao>(),
//                    Discriminator.Empty<CategoryDao>()
//                ),
//                "Tengo:\r\n" +
//                  $" - Permiso ADMIN en la categoria 'Purchases' y sus subcategorias\r\n" +
//                  $"¿Debería poder '{nameof(Create)}' una instancia de documento Word sin categoria?\r\n" +
//                  " No");

//            Assert.True(ide.Can(Create).Instance(File.Document.Word.Word1.Transform(w=>w.Category=Discriminators.Category.Purchases)),
//               "Tengo:\r\n" +
//                   $" - Permiso ADMIN en la categoria 'Purchases' y sus subcategorias\r\n" +
//                   $"¿Debería poder '{nameof(Create)}' una instancia de documento Word de la categoria 'Purchases'?\r\n" +
//                   " Si");
//            Assert.False(ide.Can(Create).Instance(File.Document.Word.Word1.Transform(w => w.Category = Discriminators.Category.Sales)),
//              "Tengo:\r\n" +
//                  $" - Permiso ADMIN en la categoria 'Purchases' y sus subcategorias\r\n" +
//                  $"¿Debería poder '{nameof(Create)}' una instancia de documento Word de la categoria 'Sales'?\r\n" +
//                  " No");
//        }
//        [Fact(DisplayName = "Rol - Grant for discriminator and denied for children")]
//        public void GrantParentDeniedChildren()
//        {
//            var ide = new IdentityDao
//            {
//                Id = "test",
//                Name = "Test",
//                Permissions = new[] {
//                    new PermissionDao {
//                        Value = true,
//                        Function = Admin.Id.ToString(),
//                        Scopes = new[] {
//                            new ScopeDao
//                            {
//                                Discriminator = Discriminators.Location.Country.Spain,
//                                Propagation = ScopePropagation.ToMe| ScopePropagation.ToInclusions
//                            }
//                        }
//                    },
//                    new PermissionDao {
//                        Value = false,
//                        Function = Read.Id.ToString(),
//                        Scopes = new[] {
//                            new ScopeDao
//                            {
//                                Discriminator = Discriminators.Location.City.Madrid,
//                                Propagation = ScopePropagation.ToMe| ScopePropagation.ToInclusions
//                            },
//                            new ScopeDao
//                            {
//                                Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<PersonDao>(),
//                                Propagation = ScopePropagation.ToMe| ScopePropagation.ToInclusions
//                            },
//                        }
//                    },
//                }
//            };
//            //Assert.False(ide.Can(Create).Instance(Person.Admin),
//            //   "Tengo:\r\n" +
//            //       $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
//            //       $" - Denegado READ en la ciudad 'Madrid' y sus sublocalizaciones\r\n" +
//            //       $"¿Debería poder '{nameof(Create)}' una instancia de persona sin ciudad?\r\n" +
//            //       " No");
//            //Assert.False(ide.Can(Create).ByAll(
//            //    Factory.Get<TypeDiscriminatorFactory>().FromType<PersonDao>(),
//            //    Discriminator.Empty<CityDao>()),
//            //   "Tengo:\r\n" +
//            //       $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
//            //       $" - Denegado READ en la ciudad 'Madrid' y sus sublocalizaciones\r\n" +
//            //       $"¿Debería poder '{nameof(Create)}' una instancia de persona sin ciudad?\r\n" +
//            //       " No");

//            Assert.False(ide.Can(Create).Instance(Person.MadridAdmin),
//               "Tengo:\r\n" +
//                   $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
//                   $" - Denegado READ 'Person' en la ciudad 'Madrid' y sus sublocalizaciones\r\n" +
//                   $"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Madrid?\r\n" +
//                   " No");
//            Assert.False(ide.Can(Create).ByAll(
//                Factory.Get<TypeDiscriminatorFactory>().FromType<PersonDao>(), 
//                Discriminators.Location.City.Madrid),
//               "Tengo:\r\n" +
//                   $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
//                   $" - Denegado READ en la ciudad 'Madrid' y sus sublocalizaciones\r\n" +
//                   $"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Madrid?\r\n" +
//                   " No");

//            Assert.True(ide.Can(Create).Instance(Person.AlcorconAdmin),
//               "Tengo:\r\n" +
//                   $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
//                   $" - Denegado READ en la ciudad 'Madrid' y sus sublocalizaciones\r\n" +
//                   $"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Alcorcon?\r\n" +
//                   " Si");
//            Assert.True(ide.Can(Create).ByAll(
//                Factory.Get<TypeDiscriminatorFactory>().FromType<PersonDao>(),
//                Discriminators.Location.City.Alcorcon),
//               "Tengo:\r\n" +
//                   $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
//                   $" - Denegado READ en la ciudad 'Madrid' y sus sublocalizaciones\r\n" +
//                   $"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Alcorcon?\r\n" +
//                   " Si");

//            Assert.True(ide.Can(Read).Type<WordDocumentDao>(),
//                 "Tengo:\r\n" +
//                   $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
//                   $" - Denegado READ en la ciudad 'Madrid' y sus sublocalizaciones\r\n" +
//                   $"¿Debería poder '{nameof(Read)}' objetos del tipo Word?\r\n" +
//                   " Si");
//        }
//        [Fact(DisplayName = "Rol - Grant for discriminator and denied for other discriminator")]
//        public void GrantADiscriminatorAndDeniedOther()
//        {
//            var ide = new IdentityDao
//            {
//                Id = "test",
//                Name = "Test",
//                Permissions = new[] {
//                    new PermissionDao {
//                        Value = true,
//                        Function = Manage.Id.ToString(),
//                        Scopes = new[] {
//                            new ScopeDao
//                            {
//                                Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<FileDao>(),
//                                Propagation = ScopePropagation.ToMe| ScopePropagation.ToInclusions
//                            }
//                        }
//                    },
//                    new PermissionDao {
//                        Value = false,
//                        Function = Read.Id.ToString(),
//                        Scopes = new[] {
//                            new ScopeDao
//                            {
//                                Discriminator = Discriminators.Location.Country.Spain,
//                                Propagation = ScopePropagation.ToMe| ScopePropagation.ToInclusions
//                            },
//                        }
//                    },
//                }
//            };
//            Assert.True(ide.Can(Read).Instance(File.Document.Word.Word1),
//               "Tengo:\r\n" +
//                   $" - Permiso MANAGE 'File' y sus derivados\r\n" +
//                   $" - Denegado READ en el pais 'Spain' y sus sublocalizaciones\r\n" +
//                   $"¿Debería poder '{nameof(Read)}' una instancia del tipo '{nameof(WordDocumentDao)}'?\r\n" +
//                   " Si");
//            Assert.True(ide.Can(Read).Type<DocumentDao>(),
//               "Tengo:\r\n" +
//                   $" - Permiso MANAGE 'File' y sus derivados\r\n" +
//                   $" - Denegado READ en el pais 'Spain' y sus sublocalizaciones\r\n" +
//                   $"¿Debería poder '{nameof(Read)}' el tipo '{nameof(DocumentDao)}'?\r\n" +
//                   " Si");

//            Assert.True(ide.Can(Read).ByAll(Factory.Get<TypeDiscriminatorFactory>().FromId(TypeDiscriminatorIds.OfficeDocument)),
//               "Tengo:\r\n" +
//                   $" - Permiso MANAGE 'File' y sus derivados\r\n" +
//                   $" - Denegado READ en el pais 'Spain' y sus sublocalizaciones\r\n" +
//                   $"¿Debería poder '{nameof(Read)}' el tipo '{TypeDiscriminatorIds.OfficeDocument}'?\r\n" +
//                   " Si");

//            //Assert.False(ide.Can(Create).Instance(Person.MadridAdmin),
//            //   "Tengo:\r\n" +
//            //       $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
//            //       $" - Denegado READ en la ciudad 'Madrid' y sus sublocalizaciones\r\n" +
//            //       $"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Madrid?\r\n" +
//            //       " No");
//            //Assert.False(ide.Can(Create).ByAll(
//            //    Factory.Get<TypeDiscriminatorFactory>().FromType<PersonDao>(),
//            //    Discriminators.Location.City.Madrid),
//            //   "Tengo:\r\n" +
//            //       $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
//            //       $" - Denegado READ en la ciudad 'Madrid' y sus sublocalizaciones\r\n" +
//            //       $"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Madrid?\r\n" +
//            //       " No");

//            //Assert.True(ide.Can(Create).Instance(Person.AlcorconAdmin),
//            //   "Tengo:\r\n" +
//            //       $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
//            //       $" - Denegado READ en la ciudad 'Madrid' y sus sublocalizaciones\r\n" +
//            //       $"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Alcorcon?\r\n" +
//            //       " Si");
//            //Assert.True(ide.Can(Create).ByAll(
//            //    Factory.Get<TypeDiscriminatorFactory>().FromType<PersonDao>(),
//            //    Discriminators.Location.City.Alcorcon),
//            //   "Tengo:\r\n" +
//            //       $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
//            //       $" - Denegado READ en la ciudad 'Madrid' y sus sublocalizaciones\r\n" +
//            //       $"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Alcorcon?\r\n" +
//            //       " Si");

//            //Assert.True(ide.Can(Read).Type<WordDocumentDao>(),
//            //     "Tengo:\r\n" +
//            //       $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
//            //       $" - Denegado READ en la ciudad 'Madrid' y sus sublocalizaciones\r\n" +
//            //       $"¿Debería poder '{nameof(Read)}' objetos del tipo Word?\r\n" +
//            //       " Si");
//        }
//    }
//}
