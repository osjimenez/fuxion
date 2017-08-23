using Fuxion.Factories;
using Fuxion.Identity.Test.Dao;
using Fuxion.Identity.Test.Helpers;
using Fuxion.Identity.Test.Mocks;
using Fuxion.Identity.Test.Repositories;
using Fuxion.Repositories;
using Fuxion.Test;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Fuxion.Identity.Functions;
using static Fuxion.Identity.Test.Context;
using static System.Extensions;

namespace Fuxion.Identity.Test
{
    public class Test2 : BaseTest
    {
        public Test2(ITestOutputHelper output) : base(output)
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

            Factory.AddInjector(new SimpleInjectorFactoryInjector(c));
        }
        void PrintTestTry(string message)
        {
            Printer.WriteLine("".PadRight(40,'=')+" TEST TRIED STARTED "+"".PadRight(40,'='));
            Printer.WriteLine(message);
        }
        [Fact(DisplayName = "Test2 - Cannot by type")]
        public void CannotByType()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[] {
                    new PermissionDao {
                        Value = true,
                        Function = Edit.Id.ToString(),
                        Scopes = new[] {
                            new ScopeDao {
                                Discriminator = Discriminators.Location.City.SanFrancisco,
                                Propagation = ScopePropagation.ToMe } } },
                    new PermissionDao {
                        Value = false,
                        Function = Edit.Id.ToString()   ,
                        Scopes = new[] {
                            new ScopeDao {
                                Discriminator = Discriminators.Location.State.California,
                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions } } }
                }.ToList()
            };
            Assert.False(ide.IsRoot2(),
                "Tengo:\r\n" +
                    $" - Concedido el permiso para {nameof(Edit)} en {nameof(Discriminators.Location.City.SanFrancisco)}\r\n" +
                    $" - Denegado el permiso para {nameof(Edit)} en {nameof(Discriminators.Location.State.California)} y sus hijos\r\n" +
                    $"¿Debería ser root?\r\n" +
                    " No");
            Assert.False(ide.Can(Edit).Instance2(Discriminators.Location.City.SanFrancisco)
                , "Tengo:\r\n" +
                    $" - Concedido el permiso para {nameof(Edit)} en {nameof(Discriminators.Location.City.SanFrancisco)}\r\n" +
                    $" - Denegado el permiso para {nameof(Edit)} en {nameof(Discriminators.Location.State.California)} y sus hijos\r\n" +
                    $"¿Debería poder {nameof(Edit)} en {nameof(Discriminators.Location.City.SanFrancisco)}?\r\n" +
                    " No");
        }
        [Fact(DisplayName = "Test2 - Grant for discriminator and denied for children")]
        public void GrantParentDeniedChildren()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[] {
                    new PermissionDao {
                        Value = true,
                        Function = Admin.Id.ToString(),
                        Scopes = new[] {
                            new ScopeDao
                            {
                                Discriminator = Discriminators.Location.Country.Spain,
                                Propagation = ScopePropagation.ToMe| ScopePropagation.ToInclusions
                            }
                        }
                    },
                    new PermissionDao {
                        Value = false,
                        Function = Read.Id.ToString(),
                        Scopes = new[] {
                            new ScopeDao
                            {
                                Discriminator = Discriminators.Location.City.Madrid,
                                Propagation = ScopePropagation.ToMe| ScopePropagation.ToInclusions
                            },
                            new ScopeDao
                            {
                                Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<PersonDao>(),
                                Propagation = ScopePropagation.ToMe| ScopePropagation.ToInclusions
                            },
                        }
                    },
                }
            };

            string permissionExplanation = "Tengo:\r\n" +
                   $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
                   $" - Denegado READ personas en la ciudad 'Madrid' y sus sublocalizaciones\r\n";

            var query = 
                $"¿Debería poder '{nameof(Create)}' una instancia de persona sin ciudad?\r\n" +
                " No";
            PrintTestTry(permissionExplanation + query);
            Assert.False(ide.Can(Create).Instance2(Person.Admin), permissionExplanation + query);

            query = 
                $"¿Debería poder '{nameof(Create)}' una instancia de persona sin ciudad?\r\n" +
                " No";
            PrintTestTry(permissionExplanation + query);
            Assert.False(ide.Can(Create).ByAll2(
                Factory.Get<TypeDiscriminatorFactory>().FromType<PersonDao>(),
                Discriminator.Empty<CityDao>()), permissionExplanation + query);

            {
                query =
                    $"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Madrid?\r\n" +
                    " No";
            
                PrintTestTry(permissionExplanation + query);
                Assert.False(ide.Can(Create).Instance2(Person.MadridAdmin), permissionExplanation + query);

                PrintTestTry(permissionExplanation + query);
                Assert.False(ide.Can(Create).ByAll2(
                    Factory.Get<TypeDiscriminatorFactory>().FromType<PersonDao>(),
                    Discriminators.Location.City.Madrid), 
                    permissionExplanation + query);
            }

            {
                query =
                    $"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Alcorcon?\r\n" +
                    " Si";
            
                PrintTestTry(permissionExplanation + query);
                Assert.True(ide.Can(Create).Instance2(Person.AlcorconAdmin), permissionExplanation + query);

                PrintTestTry(permissionExplanation + query);
                Assert.True(ide.Can(Create).ByAll2(
                    Factory.Get<TypeDiscriminatorFactory>().FromType<PersonDao>(),
                    Discriminators.Location.City.Alcorcon), permissionExplanation + query);
            }

            query = 
                $"¿Debería poder '{nameof(Read)}' objetos del tipo Word?\r\n" +
                " Si";
            PrintTestTry(permissionExplanation + query);
            Assert.True(ide.Can(Read).Type2<WordDocumentDao>(), permissionExplanation + query);
        }
        [Fact(DisplayName = "Test2 - Can by instance")]
        public void CanByInstance()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[]
                {
                    new PermissionDao {
                        Value = true,
                        Function = Edit.Id.ToString(),
                        Scopes =new[] {
                            new ScopeDao {
                                Discriminator = Discriminators.Location.State.California,
                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToExclusions }
                        }
                    }
                }
            };
            string permissionExplanation = "Tengo:\r\n" +
                   $" - Permiso EDIT en el estado 'California' y sus predecesores\r\n";

            var query = (
                $"¿Debería poder '{nameof(Edit)}' una instancia del pais '{nameof(Discriminators.Location.Country.Usa)}'?\r\n" +
                " Si").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.True(ide.Can(Edit).AllLocations2(Discriminators.Location.Country.Usa), permissionExplanation + query.Value);
        }
        [Fact(DisplayName = "Test2 - Can by instance multiple properties")]
        public void CanByInstance2()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[]
                {
                    new PermissionDao {
                        Value = true,
                        Function = Edit.Id.ToString(),
                        Scopes =new[] {
                            new ScopeDao {
                                Discriminator = Discriminators.Category.Purchases,
                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToExclusions }
                        }
                    }
                }
            };

            string permissionExplanation = "Tengo:\r\n" +
                $" - Permiso EDIT en la categoria 'Purchases' y sus categorias padre\r\n";

            var query = (
                $"¿Debería poder '{nameof(Edit)}' una instancia de 'Purchases'?\r\n" +
                " Si").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.True(ide.Can(Edit).Instance2(Discriminators.Category.Purchases), permissionExplanation + query.Value);
        }
        [Fact(DisplayName = "Test2 - Can by instance, mix root permission with others")]
        public void CanByType3()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[] {
                    new PermissionDao {
                        Value = true,
                        Function = Admin.Id.ToString(),
                    },
                    new PermissionDao {
                        Value =true,
                        Function = Admin.Id.ToString(),
                        Scopes =new[]{
                            new ScopeDao {
                                Discriminator = Discriminators.Category.Purchases,
                                Propagation = ScopePropagation.ToMe },
                        }
                    },
                }.ToList()
            };

            string permissionExplanation = "Tengo:\r\n" +
                $" - Concedido el permiso para {nameof(Admin)} sin restricciones\r\n" +
                $" - Concedido el permiso para {nameof(Admin)} en {nameof(Discriminators.Category.Purchases)}\r\n";

            var query = (
                $"¿Debería poder {nameof(Read)} en {nameof(File.Document.Word.Word1)}?\r\n" +
                " Si").AsDisposable();

            PrintTestTry(permissionExplanation + query.Value);
            Assert.True(ide.Can(Read).Instance2(File.Document.Word.Word1), permissionExplanation + query.Value);

             query = (
                $"¿Debería poder {nameof(Read)} alguna cosa?\r\n" +
                " Si").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.True(ide.Can(Read).Something2(), permissionExplanation + query.Value);
        }
        [Fact(DisplayName = "Test2 - Can by type, different discriminator type")]
        public void CanByType2()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[] {
                    new PermissionDao {
                        Value =true,
                        Function = Admin.Id.ToString(),
                        Scopes =new[]{
                            new ScopeDao {
                                Discriminator = Discriminators.Category.Purchases,
                                Propagation = ScopePropagation.ToMe },
                        }
                    },
                }.ToList()
            };

            string permissionExplanation = "Tengo:\r\n" +
                $" - Concedido el permiso para {nameof(Admin)} en {nameof(Discriminators.Category.Purchases)}\r\n";

            var query = (
                $"¿Debería poder {nameof(Read)} objetos de tipo '{nameof(WordDocumentDao)}'?\r\n" +
                " Si").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.True(ide.Can(Read).Type2<WordDocumentDao>(), permissionExplanation + query.Value);
        }
        [Fact(DisplayName = "Test2 - Can by type, same discriminator type")]
        public void CanByType()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[] {
                    new PermissionDao {
                        Value =true,
                        Function = Admin.Id.ToString(),
                        Scopes =new[]{
                            new ScopeDao {
                                Discriminator = Discriminators.Category.Purchases,
                                Propagation = ScopePropagation.ToMe },
                            new ScopeDao {
                                Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<BaseDao>(),
                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions },
                        }
                    },
                }.ToList()
            };

            string permissionExplanation = "Tengo:\r\n" +
                $" - Concedido el permiso para {nameof(Admin)} la categoria {Discriminators.Category.Purchases} del tipo {nameof(BaseDao)} y derivados\r\n";

            var query = (
                $"¿Debería poder {nameof(Read)} en {nameof(WordDocumentDao)}?\r\n" +
                " Si").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.True(ide.Can(Read).Type<WordDocumentDao>(), permissionExplanation + query.Value);

            query = (
                $"¿Debería poder {nameof(Admin)} alguna cosa?\r\n" +
                " Si").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.True(ide.Can(Admin).Something2(), permissionExplanation + query.Value);
        }
        [Fact(DisplayName = "Test2 - Cannot admin something")]
        public void CannotAdminSomething()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[]
                {
                    new PermissionDao {
                        Value = true,
                        Function = Edit.Id.ToString(),
                        Scopes =new[] {
                            new ScopeDao {
                                Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<CategoryDao>(),
                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions }
                        }
                    },
                    new PermissionDao {
                        Value = false,
                        Function = Read.Id.ToString(),
                    },
                }
            };

            string permissionExplanation = "Tengo:\r\n" +
                $" - Concedido el permiso para {nameof(Edit)} el tipo {nameof(CategoryDao)} y derivados\r\n" +
                $" - Denegado el permiso para {nameof(Read)} cualquier cosa\r\n";

            var query = (
                $"¿Debería poder {nameof(Admin)} alguna cosa?\r\n" +
                " No").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.False(ide.Can(Admin).Something2(), permissionExplanation + query.Value);
        }
        [Fact(DisplayName = "Test2 - Cannot by function")]
        public void CannotByFunction()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[]
                {
                    new PermissionDao {
                        Value = true,
                        Function = Edit.Id.ToString(),
                        Scopes = new[] {
                            new ScopeDao {
                                Discriminator = Discriminators.Category.Purchases,
                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToExclusions }
                        }
                    }
                },
            };

            string permissionExplanation = "Tengo:\r\n" +
                $" - Concedido el permiso para {nameof(Edit)} en {nameof(Discriminators.Category.Purchases)}\r\n";

            var query = (
                $"¿Debería poder {nameof(Delete)} en {nameof(Discriminators.Category.Purchases)}?\r\n" +
                " No").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.False(ide.Can(Delete).Instance2(Discriminators.Category.Purchases), permissionExplanation + query.Value);

            query = (
                $"¿Debería poder {nameof(Read)} en {nameof(Discriminators.Category.Purchases)}?\r\n" +
                " Si").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.True(ide.Can(Read).Instance2(Discriminators.Category.Purchases), permissionExplanation + query.Value);

            query = (
                $"¿Debería poder {nameof(Delete)} en {nameof(Discriminators.Category.Purchases)}?\r\n" +
                " No").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.False(ide.Can(Delete).Type2<WordDocumentDao>(), permissionExplanation + query.Value);

            query = (
                $"¿Debería poder {nameof(Read)} en {nameof(Discriminators.Category.Purchases)}?\r\n" +
                " Si").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.True(ide.Can(Read).Type2<WordDocumentDao>(), permissionExplanation + query.Value);
        }
        [Fact(DisplayName = "Test2 - Cannot by instance")]
        public void CannotByInstance()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[]
                {
                    new PermissionDao {
                        Value = true,
                        Function =Edit.Id.ToString(),
                        Scopes =new[] {
                            new ScopeDao {
                                Discriminator = Discriminators.Category.Purchases,
                                Propagation = ScopePropagation.ToMe }
                        }
                    }
                }
            };

            string permissionExplanation = "Tengo:\r\n" +
                $" - Concedido el permiso para {nameof(Edit)} en {nameof(Discriminators.Category.Purchases)}\r\n";

            var query = (
                $"¿Debería poder {nameof(Edit)} una instancia de documento de Word sin categoria?\r\n" +
                " No").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.False(ide.Can(Edit).Instance2(File.Document.Word.Word1), permissionExplanation + query.Value);
        }
        [Fact(DisplayName = "Test2 - Cannot for different discriminator")]
        public void CannotForDifferent()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[]
                {
                    new PermissionDao {
                        Value = true,
                        Function = Edit.Id.ToString(),
                        Scopes =new[] {
                            new ScopeDao {
                                Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<CategoryDao>(),
                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions }
                        }
                    }
                }
            };

            string permissionExplanation = "Tengo:\r\n" +
                $" - Concedido el permiso para {nameof(Edit)} el tipo {nameof(CategoryDao)} y derivados\r\n";

            var query = (
                $"¿Debería poder {nameof(Read)} en {nameof(Discriminators.Location.City.Buffalo)}?\r\n" +
                " No").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.False(ide.Can(Read).Instance2(Discriminators.Location.City.Buffalo), permissionExplanation + query.Value);
        }
        [Fact(DisplayName = "Test2 - Cannot for different discriminator2")]
        public void CannotForDifferent2()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[]
                {
                    new PermissionDao {
                        Value = true,
                        Function = Edit.Id.ToString(),
                        Scopes =new[] {
                            new ScopeDao {
                                Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<CategoryDao>(),
                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions },
                        }
                    },
                    new PermissionDao {
                        Value = false,
                        Function = Read.Id.ToString(),
                        Scopes =new[] {
                            new ScopeDao {
                                Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<CategoryDao>(),
                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions }
                        }
                    },
                }
            };

            string permissionExplanation = "Tengo:\r\n" +
                $" - Concedido el permiso para {nameof(Edit)} el tipo {nameof(CategoryDao)} y derivados\r\n" +
                $" - Denegado el permiso para {nameof(Read)} el tipo {nameof(CategoryDao)} y derivados\r\n";

            var query = (
                $"¿Debería poder {nameof(Read)} la categoria '{nameof(Discriminators.Category.Purchases)}'?\r\n" +
                " No").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.False(ide.Can(Read).Instance2(Discriminators.Category.Purchases), permissionExplanation + query.Value);
        }
        [Fact(DisplayName = "Test2 - Empty discriminator")]
        public void EmptyDiscriminator()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[] {
                    new PermissionDao {
                        Value = true,
                        Function = Admin.Id.ToString(),
                        Scopes = new[] {
                            new ScopeDao
                            {
                                Discriminator = Discriminators.Category.Purchases,
                                Propagation = ScopePropagation.ToMe| ScopePropagation.ToInclusions
                            }
                        }
                    }
                }.ToList()
            };

            string permissionExplanation = "Tengo:\r\n" +
                $" - Permiso ADMIN en la categoria 'Purchases' y sus subcategorias\r\n";

            var query = (
                $"¿Debería poder '{nameof(Create)}' una instancia de documento Word sin categoria?\r\n" +
                " No").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.False(ide.Can(Create).Instance2(File.Document.Word.Word1), permissionExplanation + query.Value);
            PrintTestTry(permissionExplanation + query.Value);
            Assert.False(ide.Can(Create).ByAll2(
                    Factory.Get<TypeDiscriminatorFactory>().FromType<WordDocumentDao>(),
                    Discriminator.Empty<CategoryDao>()), permissionExplanation + query.Value);

            query = (
               $"¿Debería poder '{nameof(Create)}' una instancia de documento Word de la categoria 'Purchases'?\r\n" +
               " Si").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.True(ide.Can(Create).Instance2(File.Document.Word.Word1.Transform(w => w.Category = Discriminators.Category.Purchases)), permissionExplanation + query.Value);

            query = (
               $"¿Debería poder '{nameof(Create)}' una instancia de documento Word de la categoria 'Sales'?\r\n" +
               " No").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.False(ide.Can(Create).Instance2(File.Document.Word.Word1.Transform(w => w.Category = Discriminators.Category.Sales)), permissionExplanation + query.Value);
        }
        [Fact(DisplayName = "Test2 - Grant for discriminator and denied for other discriminator")]
        public void GrantForDiscriminatorAndDeniedOther()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[] {
                    new PermissionDao {
                        Value = true,
                        Function = Manage.Id.ToString(),
                        Scopes = new[] {
                            new ScopeDao
                            {
                                Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<FileDao>(),
                                Propagation = ScopePropagation.ToMe| ScopePropagation.ToInclusions
                            }
                        }
                    },
                    new PermissionDao {
                        Value = false,
                        Function = Read.Id.ToString(),
                        Scopes = new[] {
                            new ScopeDao
                            {
                                Discriminator = Discriminators.Location.Country.Spain,
                                Propagation = ScopePropagation.ToMe| ScopePropagation.ToInclusions
                            },
                            // Si pongo esto, pasa el TEST
                            //new ScopeDao
                            //{
                            //    Discriminator = TypeDiscriminator.Empty,
                            //    Propagation = ScopePropagation.ToMe| ScopePropagation.ToInclusions
                            //},
                        }
                    },
                }
            };

            string permissionExplanation = "Tengo:\r\n" +
                $" - Permiso MANAGE para el tipo 'File' y sus derivados\r\n" +
                $" - Denegado READ en el pais 'Spain' y sus sublocalizaciones\r\n";

            var query = (
                $"¿Debería poder '{nameof(Read)}' una instancia del tipo '{nameof(WordDocumentDao)}' sin categoria?\r\n" +
                " Si").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.True(ide.Can(Read).Instance2(File.Document.Word.Word1), permissionExplanation + query.Value);

            query = (
                $"¿Debería poder '{nameof(Read)}' el tipo '{nameof(DocumentDao)}'?\r\n" +
                " Si").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.True(ide.Can(Read).Type2<DocumentDao>(), permissionExplanation + query.Value);

            query = (
                $"¿Debería poder '{nameof(Read)}' el tipo '{TypeDiscriminatorIds.OfficeDocument}'?\r\n" +
                " Si").AsDisposable();
            PrintTestTry(permissionExplanation + query.Value);
            Assert.True(ide.Can(Read).ByAll2(Factory.Get<TypeDiscriminatorFactory>().FromId(TypeDiscriminatorIds.OfficeDocument)), permissionExplanation + query.Value);

            //Assert.False(ide.Can(Create).Instance(Person.MadridAdmin),
            //   "Tengo:\r\n" +
            //       $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
            //       $" - Denegado READ en la ciudad 'Madrid' y sus sublocalizaciones\r\n" +
            //       $"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Madrid?\r\n" +
            //       " No");
            //Assert.False(ide.Can(Create).ByAll(
            //    Factory.Get<TypeDiscriminatorFactory>().FromType<PersonDao>(),
            //    Discriminators.Location.City.Madrid),
            //   "Tengo:\r\n" +
            //       $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
            //       $" - Denegado READ en la ciudad 'Madrid' y sus sublocalizaciones\r\n" +
            //       $"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Madrid?\r\n" +
            //       " No");

            //Assert.True(ide.Can(Create).Instance(Person.AlcorconAdmin),
            //   "Tengo:\r\n" +
            //       $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
            //       $" - Denegado READ en la ciudad 'Madrid' y sus sublocalizaciones\r\n" +
            //       $"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Alcorcon?\r\n" +
            //       " Si");
            //Assert.True(ide.Can(Create).ByAll(
            //    Factory.Get<TypeDiscriminatorFactory>().FromType<PersonDao>(),
            //    Discriminators.Location.City.Alcorcon),
            //   "Tengo:\r\n" +
            //       $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
            //       $" - Denegado READ en la ciudad 'Madrid' y sus sublocalizaciones\r\n" +
            //       $"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Alcorcon?\r\n" +
            //       " Si");

            //Assert.True(ide.Can(Read).Type<WordDocumentDao>(),
            //     "Tengo:\r\n" +
            //       $" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
            //       $" - Denegado READ en la ciudad 'Madrid' y sus sublocalizaciones\r\n" +
            //       $"¿Debería poder '{nameof(Read)}' objetos del tipo Word?\r\n" +
            //       " Si");
        }
        [Fact(DisplayName = "Test2 - Root permission")]
        public void RootPermission()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[] {
                    new PermissionDao {
                        Value = true,
                        Function = Admin.Id.ToString(),
                    }
                }.ToList()
            };

            string permissionExplanation = "Tengo:\r\n" +
                $" - Permiso ROOT\r\n";

            var query = 
                $"¿Debería ser root?\r\n" +
                " Si";
            PrintTestTry(permissionExplanation + query);
            Assert.True(ide.IsRoot2(), permissionExplanation + query);

            query =
                $"¿Debería poder '{nameof(Admin)}' cualquier cosa?\r\n" +
                " Si";
            PrintTestTry(permissionExplanation + query);
            Assert.True(ide.Can(Admin).Anything2(), permissionExplanation + query);

            query =
                $"¿Debería poder '{nameof(Admin)}' alguna cosa?\r\n" +
                " Si";
            PrintTestTry(permissionExplanation + query);
            Assert.True(ide.Can(Admin).Something2(), permissionExplanation + query);

            query =
                $"¿Debería poder '{nameof(Edit)}' en '{nameof(File.Document.Word.Word1)}'?\r\n" +
                " Si";
            PrintTestTry(permissionExplanation + query);
            Assert.True(ide.Can(Edit).Instance2(File.Document.Word.Word1), permissionExplanation + query);

            query =
                $"¿Debería poder '{nameof(Edit)}' las '{nameof(CityDao)}'?\r\n" +
                " Si";
            PrintTestTry(permissionExplanation + query);
            Assert.True(ide.Can(Edit).Type2<CityDao>(), permissionExplanation + query);
        }
        [Fact(DisplayName = "Test2 - No permissions")]
        public void NoPermissions()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
            };

            string permissionExplanation = "Tengo:\r\n" +
                $" - Ningún permiso\r\n";

            var query =
                $"¿Debería ser root?\r\n" +
                " No";
            PrintTestTry(permissionExplanation + query);
            Assert.False(ide.IsRoot2(), permissionExplanation + query);

            query =
                $"¿Debería poder '{nameof(Read)}' alguna cosa?\r\n" +
                " No";
            PrintTestTry(permissionExplanation + query);
            Assert.False(ide.Can(Read).Something2(), permissionExplanation + query);

            query =
                $"¿Debería poder {nameof(Edit)} en {nameof(Discriminators.Location.City.SanFrancisco)}?\r\n" +
                " No";
            PrintTestTry(permissionExplanation + query);
            Assert.False(ide.Can(Edit).Instance2(Discriminators.Location.City.SanFrancisco), permissionExplanation + query);
        }
        [Fact(DisplayName = "Test2 - Only read permission")]
        public void OnlyReadPermission()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[] {
                    new PermissionDao {
                        Value = true,
                        Function = Read.Id.ToString(),
                    }
                }.ToList()
            };

            string permissionExplanation = "Tengo:\r\n" +
                $" - Permiso ONLY READ\r\n";

            var query =
                $"¿Debería poder '{nameof(Read)}' las '{nameof(CityDao)}'?\r\n" +
                " Si";
            PrintTestTry(permissionExplanation + query);
            Assert.True(ide.Can(Read).Type2<CityDao>(), permissionExplanation + query);

            query =
                $"¿Debería poder '{nameof(Create)}' las '{nameof(CityDao)}'?\r\n" +
                " No";
            PrintTestTry(permissionExplanation + query);
            Assert.False(ide.Can(Create).Type2<CityDao>(), permissionExplanation + query);
        }
    }
}