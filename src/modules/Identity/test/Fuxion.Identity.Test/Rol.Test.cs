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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Fuxion.Identity.Functions;
using static Fuxion.Identity.Test.Context;
using static System.Extensions;

namespace Fuxion.Identity.Test.Rol
{
    public class RolTest : BaseTest
    {
        public RolTest(ITestOutputHelper output) : base(output)
        {
            Container c = new Container();

            // TypeDiscriminators
            c.RegisterSingleton(new TypeDiscriminatorFactory().Transform(fac =>
            {
                fac.RegisterTree<BaseDao>(typeof(BaseDao).Assembly.DefinedTypes.Except(new[] { typeof(PermissionDao).GetTypeInfo() }).ToArray());
                //fac.RegisterTree<BaseDao>(typeof(BaseDao).Assembly.DefinedTypes.ToArray());
                return fac;
            }));
            // IdentityManager
            c.Register<IPasswordProvider, PasswordProviderMock>();
            c.RegisterSingleton<ICurrentUserNameProvider>(new GenericCurrentUserNameProvider(() => Context.Rols.Identity.Root.UserName));
            c.RegisterSingleton<IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>>(new IdentityMemoryTestRepository());
            c.Register<IdentityManager>();
            Admin.Name = "Administrar";
            Manage.Name = "Gestionar";
            Delete.Name = "Borrar";
            Create.Name = "Crear";
            Edit.Name = "Editar";
            Read.Name = "Leer";

            Factory.AddInjector(new SimpleInjectorFactoryInjector(c));
        }
        void PrintTestTriedStarted(string message)
        {
            Printer.WriteLine("".PadRight(40,'=')+" TEST TRIED STARTED "+"".PadRight(40,'='));
            Printer.WriteLine(message);
        }
        [Fact(DisplayName = "Rol - Cannot by type")]
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

            string permissionExplanation = "Tengo:\r\n" +
                $" - Concedido el permiso para {nameof(Edit)} en {nameof(Discriminators.Location.City.SanFrancisco)}\r\n" +
                $" - Denegado el permiso para {nameof(Edit)} en {nameof(Discriminators.Location.State.California)} y sus hijos\r\n";

            var query =
                $"¿Debería ser root?\r\n" +
                " No";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(ide.IsRoot(), permissionExplanation + query);

            query =
                $"¿Debería poder {nameof(Edit)} en {nameof(Discriminators.Location.City.SanFrancisco)}?\r\n" +
                " No";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(ide.Can(Edit).Instance(Discriminators.Location.City.SanFrancisco), permissionExplanation + query);
        }
        [Fact(DisplayName = "Rol - Grant for discriminator and denied for children")]
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
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(ide.Can(Create).Instance(Person.Admin), permissionExplanation + query);

            query = 
                $"¿Debería poder '{nameof(Create)}' una instancia de persona sin ciudad?\r\n" +
                " No";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(ide.Can(Create).ByAll(
                Factory.Get<TypeDiscriminatorFactory>().FromType<PersonDao>(),
                Discriminator.Empty<CityDao>()), permissionExplanation + query);

            {
                query =
                    $"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Madrid?\r\n" +
                    " No";
            
                PrintTestTriedStarted(permissionExplanation + query);
                Assert.False(ide.Can(Create).Instance(Person.MadridAdmin), permissionExplanation + query);

                PrintTestTriedStarted(permissionExplanation + query);
                Assert.False(ide.Can(Create).ByAll(
                    Factory.Get<TypeDiscriminatorFactory>().FromType<PersonDao>(),
                    Discriminators.Location.City.Madrid), 
                    permissionExplanation + query);
            }

            {
                query =
                    $"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Alcorcon?\r\n" +
                    " Si";
            
                PrintTestTriedStarted(permissionExplanation + query);
                Assert.True(ide.Can(Create).Instance(Person.AlcorconAdmin), permissionExplanation + query);

                PrintTestTriedStarted(permissionExplanation + query);
                Assert.True(ide.Can(Create).ByAll(
                    Factory.Get<TypeDiscriminatorFactory>().FromType<PersonDao>(),
                    Discriminators.Location.City.Alcorcon), permissionExplanation + query);
            }

            var source = new[] { Person.AlcorconAdmin, Person.MadridAdmin };
            query =
                $"¿Debería filtrar para '{nameof(Create)}' una instancia de pesona con la ciudad Alcorcon?\r\n" +
                " No";
            PrintTestTriedStarted(permissionExplanation + query);
            var source1 = source.AuthorizedTo(ide, Create);
            using (Printer.Indent2("Source filtered results:"))
            {
                foreach (var per in source1) {
                    Printer.WriteLine("Person: " + per);
                }
            }
            Assert.True(source1.Contains(Person.AlcorconAdmin), permissionExplanation + query);

            query =
                $"¿Debería filtrar para '{nameof(Create)}' una instancia de pesona con la ciudad Madrid?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(source.AuthorizedTo(ide, Create).Contains(Person.MadridAdmin), permissionExplanation + query);

            query = 
                $"¿Debería poder '{nameof(Read)}' objetos del tipo Word?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Read).Type<WordDocumentDao>(), permissionExplanation + query);
        }
        [Fact(DisplayName = "Rol - Can by instance")]
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
            PrintTestTriedStarted(permissionExplanation + query.Value);
            Assert.True(ide.Can(Edit).AllLocations2(Discriminators.Location.Country.Usa), permissionExplanation + query.Value);
        }
        [Fact(DisplayName = "Rol - Can by instance multiple properties")]
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

            var query = 
                $"¿Debería poder '{nameof(Edit)}' una instancia de 'Purchases'?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Edit).Instance(Discriminators.Category.Purchases), permissionExplanation + query);
        }
        [Fact(DisplayName = "Rol - Can by instance, mix root permission with others")]
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

            var query = 
                $"¿Debería poder {nameof(Read)} en {nameof(File.Document.Word.Word1)}?\r\n" +
                " Si";

            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Read).Instance(File.Document.Word.Word1), permissionExplanation + query);

             query = 
                $"¿Debería poder {nameof(Read)} alguna cosa?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Read).Something(), permissionExplanation + query);
        }
        [Fact(DisplayName = "Rol - Can by type, different discriminator type")]
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

            var query = 
                $"¿Debería poder {nameof(Read)} objetos de tipo '{nameof(WordDocumentDao)}'?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Read).Type<WordDocumentDao>(), permissionExplanation + query);
        }
        [Fact(DisplayName = "Rol - Can by type, same discriminator type")]
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

            var query = 
                $"¿Debería poder {nameof(Read)} en {nameof(WordDocumentDao)}?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Read).Type<WordDocumentDao>(), permissionExplanation + query);

            query = 
                $"¿Debería poder {nameof(Admin)} alguna cosa?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Admin).Something(), permissionExplanation + query);
        }
        [Fact(DisplayName = "Rol - Cannot admin something")]
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

            var query = 
                $"¿Debería poder {nameof(Admin)} alguna cosa?\r\n" +
                " No";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(ide.Can(Admin).Something(), permissionExplanation + query);
        }
        [Fact(DisplayName = "Rol - Cannot by function")]
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

            var query = 
                $"¿Debería poder {nameof(Delete)} en {nameof(Discriminators.Category.Purchases)}?\r\n" +
                " No";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(ide.Can(Delete).Instance(Discriminators.Category.Purchases), permissionExplanation + query);

            query = 
                $"¿Debería poder {nameof(Read)} en {nameof(Discriminators.Category.Purchases)}?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Read).Instance(Discriminators.Category.Purchases), permissionExplanation + query);

            query = 
                $"¿Debería poder {nameof(Delete)} en {nameof(Discriminators.Category.Purchases)}?\r\n" +
                " No";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(ide.Can(Delete).Type<WordDocumentDao>(), permissionExplanation + query);

            query = 
                $"¿Debería poder {nameof(Read)} en {nameof(Discriminators.Category.Purchases)}?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Read).Type<WordDocumentDao>(), permissionExplanation + query);
        }
        [Fact(DisplayName = "Rol - Cannot by instance")]
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

            var query = 
                $"¿Debería poder {nameof(Edit)} una instancia de documento de Word sin categoria?\r\n" +
                " No";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(ide.Can(Edit).Instance(File.Document.Word.Word1), permissionExplanation + query);
        }
        [Fact(DisplayName = "Rol - Cannot for different discriminator")]
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

            var query = 
                $"¿Debería poder {nameof(Read)} en {nameof(Discriminators.Location.City.Buffalo)}?\r\n" +
                " No";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(ide.Can(Read).Instance(Discriminators.Location.City.Buffalo), permissionExplanation + query);
        }
        [Fact(DisplayName = "Rol - Cannot for different discriminator2")]
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

            var query = 
                $"¿Debería poder {nameof(Read)} la categoria '{nameof(Discriminators.Category.Purchases)}'?\r\n" +
                " No";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(ide.Can(Read).Instance(Discriminators.Category.Purchases), permissionExplanation + query);
        }
        [Fact(DisplayName = "Rol - Empty discriminator")]
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

            var query = 
                $"¿Debería poder '{nameof(Create)}' una instancia de documento Word sin categoria?\r\n" +
                " No";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(ide.Can(Create).Instance(File.Document.Word.Word1), permissionExplanation + query);
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(ide.Can(Create).ByAll(
                    Factory.Get<TypeDiscriminatorFactory>().FromType<WordDocumentDao>(),
                    Discriminator.Empty<CategoryDao>()), permissionExplanation + query);

            query = 
               $"¿Debería poder '{nameof(Create)}' una instancia de documento Word de la categoria 'Purchases'?\r\n" +
               " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Create).Instance(File.Document.Word.Word1.Transform(w => w.Category = Discriminators.Category.Purchases)), permissionExplanation + query);

            query = 
               $"¿Debería poder '{nameof(Create)}' una instancia de documento Word de la categoria 'Sales'?\r\n" +
               " No";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(ide.Can(Create).Instance(File.Document.Word.Word1.Transform(w => w.Category = Discriminators.Category.Sales)), permissionExplanation + query);
        }
        [Fact(DisplayName = "Rol - Grant for discriminator and denied for other discriminator")]
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
                            // Si agrego este segundo discriminador, pasa el TEST.
                            // La cosa es, cuando tenemos un permiso discriminado por un tipo de discriminador y en la query tengo otro tipo de discriminador .. pasa el permiso el filtro?
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

            var query = 
                $"¿Debería poder '{nameof(Read)}' una instancia del tipo '{nameof(WordDocumentDao)}' sin categoria?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Read).Instance(File.Document.Word.Word1), permissionExplanation + query);

            query = 
                $"¿Debería poder '{nameof(Read)}' el tipo '{nameof(DocumentDao)}'?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Read).Type<DocumentDao>(), permissionExplanation + query);

            query = 
                $"¿Debería poder '{nameof(Read)}' el tipo '{TypeDiscriminatorIds.OfficeDocument}'?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Read).ByAll(Factory.Get<TypeDiscriminatorFactory>().FromId(TypeDiscriminatorIds.OfficeDocument)), permissionExplanation + query);


        }
        [Fact(DisplayName = "Rol - Unknown type")]
        public void UnknownType()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test"
            };

            string permissionExplanation = "Tengo:\r\n" +
                $" - Ningún permiso\r\n";

            var query =
                $"¿Debería poder '{nameof(Edit)}' del tipo '{nameof(PermissionDao)}'?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Edit).Type<PermissionDao>(), permissionExplanation + query);
        }
        [Fact(DisplayName = "Rol - Root permission")]
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
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.IsRoot(), permissionExplanation + query);

            query =
                $"¿Debería poder '{nameof(Admin)}' cualquier cosa?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Admin).Anything(), permissionExplanation + query);

            query =
                $"¿Debería poder '{nameof(Admin)}' alguna cosa?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Admin).Something(), permissionExplanation + query);

            query =
                $"¿Debería poder '{nameof(Edit)}' en '{nameof(File.Document.Word.Word1)}'?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Edit).Instance(File.Document.Word.Word1), permissionExplanation + query);

            query =
                $"¿Debería poder '{nameof(Edit)}' las '{nameof(CityDao)}'?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Edit).Type<CityDao>(), permissionExplanation + query);
        }
        [Fact(DisplayName = "Rol - No permissions")]
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
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(ide.IsRoot(), permissionExplanation + query);

            query =
                $"¿Debería poder '{nameof(Read)}' alguna cosa?\r\n" +
                " No";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(ide.Can(Read).Something(), permissionExplanation + query);

            query =
                $"¿Debería poder {nameof(Edit)} en {nameof(Discriminators.Location.City.SanFrancisco)}?\r\n" +
                " No";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(ide.Can(Edit).Instance(Discriminators.Location.City.SanFrancisco), permissionExplanation + query);
        }
        [Fact(DisplayName = "Rol - Only read permission")]
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
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Read).Type<CityDao>(), permissionExplanation + query);

            query =
                $"¿Debería poder '{nameof(Create)}' las '{nameof(CityDao)}'?\r\n" +
                " No";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.False(ide.Can(Create).Type<CityDao>(), permissionExplanation + query);
        }
        [Fact(DisplayName = "Rol - Type not discriminated")]
        public void TypeNotDiscriminated()
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
                $"¿Debería poder '{nameof(Admin)}' los '{nameof(PackageDao)}' (estan deshabilitados)?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Admin).Type<PackageDao>(), permissionExplanation + query);
            Assert.True(ide.Can(Admin).Instance(File.Package.Package1), permissionExplanation + query);
            Assert.True(new[] { File.Package.Package1 }.AuthorizedTo(ide, Admin).Any(), permissionExplanation + query);
        }
        [Fact(DisplayName = "Rol - Type discriminator related to any permission scope")]
        public void TypeDiscriminatorRelatedToAnyPermissionScope()
        {
            var ide = new IdentityDao
            {
                Id = "test",
                Name = "Test",
                Permissions = new[] {
                    new PermissionDao {
                        Value = true,
                        Function = Admin.Id.ToString(),
                        Scopes = new[]
                        {
                            new ScopeDao
                            {
                                Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<RolDao>(),
                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions
                            }
                        }
                    },
                    new PermissionDao {
                        Value = true,
                        Function = Admin.Id.ToString(),
                        Scopes = new[]
                        {
                            new ScopeDao
                            {
                                Discriminator = Factory.Get<TypeDiscriminatorFactory>().FromType<CityDao>(),
                                Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions
                            }
                        }
                    }
                }.ToList()
            };

            string permissionExplanation = "Tengo:\r\n" +
                $" - Permiso ADMIN para el tipo 'Rol' y sus derivados\r\n"+
                $" - Permiso ADMIN para el tipo 'City' y sus derivados\r\n";

            var query =
                $"¿Debería poder '{nameof(Admin)}' la instancia '{nameof(Context.Rols.Identity.Customer)}' del tipo '{nameof(RolDao)}'?\r\n" +
                " Si";
            PrintTestTriedStarted(permissionExplanation + query);
            Assert.True(ide.Can(Admin).Instance(Context.Rols.Identity.Customer), permissionExplanation + query);

            //query =
            //    $"¿Debería poder '{nameof(Create)}' las '{nameof(CityDao)}'?\r\n" +
            //    " No";
            //PrintTestTriedStarted(permissionExplanation + query);
            //Assert.False(ide.Can(Create).Type<CityDao>(), permissionExplanation + query);
        }
    }
}