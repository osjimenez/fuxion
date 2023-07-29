using System.Reflection;
using Fuxion.Identity.Test.Dao;
using Fuxion.Identity.Test.Mocks;
using Fuxion.Repositories;
using Fuxion.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Identity.Test.Rol;

using static Functions;

public class RolTest : BaseTest<RolTest>
{
	public RolTest(ITestOutputHelper output) : base(output)
	{
		Context.Initialize();
		services = new();
		Singleton.AddOrSkip(new TypeDiscriminatorFactory().Transform(fac => {
			fac.RegisterTree<BaseDao>(typeof(BaseDao).Assembly.DefinedTypes.Except(new[] { typeof(PermissionDao).GetTypeInfo() }).ToArray());
			return fac;
		}));
		services.AddSingleton(Singleton.Get<TypeDiscriminatorFactory>());
		services.AddTransient<IPasswordProvider, PasswordProviderMock>();
		services.AddSingleton<ICurrentUserNameProvider>(new GenericCurrentUserNameProvider(() => Identities.Root.UserName));
		services.AddSingleton<IKeyValueRepository<string, IIdentity>>(new IdentityMemoryTestRepository());
		services.AddTransient<IdentityManager>();
		Admin.Name = "Administrar";
		Manage.Name = "Gestionar";
		Delete.Name = "Borrar";
		Create.Name = "Crear";
		Edit.Name = "Editar";
		Read.Name = "Leer";
	}
	readonly ServiceCollection services;
	void PrintTestTriedStarted(string message)
	{
		Printer.WriteLine("".PadRight(40, '=') + " TEST TRIED STARTED " + "".PadRight(40, '='));
		Printer.WriteLine(message);
	}
	[Fact(DisplayName = "Rol - Can by instance")]
	public void CanByInstance()
	{
		var ide = new IdentityDao("test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Edit.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", States.California, ScopePropagation.ToMe | ScopePropagation.ToExclusions)
				}
			}
		};
		var permissionExplanation = "Tengo:\r\n" +
			" - Permiso EDIT en el estado 'California' y sus predecesores\r\n";
		var query = (
			$"¿Debería poder '{nameof(Edit)}' una instancia del pais '{nameof(Countries.Usa)}'?\r\n" +
			" Si").AsDisposable();
		PrintTestTriedStarted(permissionExplanation + query.Value);
		Assert.True(ide.Can(Edit).AllLocations2(Countries.Usa), permissionExplanation + query.Value);
	}
	[Fact(DisplayName = "Rol - Can by instance multiple properties")]
	public void CanByInstance2()
	{
		var ide = new IdentityDao("test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Edit.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", Categories.Purchases, ScopePropagation.ToMe | ScopePropagation.ToExclusions)
				}
			}
		};
		var permissionExplanation = "Tengo:\r\n" +
			" - Permiso EDIT en la categoria 'Purchases' y sus categorias padre\r\n";
		var query =
			$"¿Debería poder '{nameof(Edit)}' una instancia de 'Purchases'?\r\n" +
			" Si";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.True(ide.Can(Edit).Instance(Categories.Purchases), permissionExplanation + query);
	}
	[Fact(DisplayName = "Rol - Can by type, same discriminator type")]
	public void CanByType()
	{
		var pro = services.BuildServiceProvider();
		var ide = new IdentityDao("test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Admin.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", Categories.Purchases, ScopePropagation.ToMe),
					new ScopeDao("", "", pro.GetRequiredService<TypeDiscriminatorFactory>().FromType<BaseDao>(true)
#if NETSTABDARD2_0 || NET462
						?? throw new NullReferenceException()
#endif
						, ScopePropagation.ToMe | ScopePropagation.ToInclusions)
				}
			}
		}.ToList();
		var permissionExplanation = "Tengo:\r\n" +
			$" - Concedido el permiso para {nameof(Admin)} la categoria {Categories.Purchases} del tipo {nameof(BaseDao)} y derivados\r\n";
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
	[Fact(DisplayName = "Rol - Can by type, different discriminator type")]
	public void CanByType2()
	{
		var ide = new IdentityDao("test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Admin.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", Categories.Purchases, ScopePropagation.ToMe)
				}
			}
		}.ToList();
		var permissionExplanation = "Tengo:\r\n" +
			$" - Concedido el permiso para {nameof(Admin)} en {nameof(Categories.Purchases)}\r\n";
		var query =
			$"¿Debería poder {nameof(Read)} objetos de tipo '{nameof(WordDocumentDao)}'?\r\n" +
			" Si";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.True(ide.Can(Read).Type<WordDocumentDao>(), permissionExplanation + query);
	}
	[Fact(DisplayName = "Rol - Can by instance, mix root permission with others")]
	public void CanByType3()
	{
		var ide = new IdentityDao("test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Admin.Id.ToString() ?? "") {
				Value = true
			},
			new PermissionDao("", "", ide, Admin.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", Categories.Purchases, ScopePropagation.ToMe)
				}
			}
		};
		var permissionExplanation = "Tengo:\r\n" +
			$" - Concedido el permiso para {nameof(Admin)} sin restricciones\r\n" +
			$" - Concedido el permiso para {nameof(Admin)} en {nameof(Categories.Purchases)}\r\n";
		var query =
			$"¿Debería poder {nameof(Read)} en {nameof(Documents.Word1)}?\r\n" +
			" Si";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.True(ide.Can(Read).Instance(Documents.Word1), permissionExplanation + query);
		query =
			$"¿Debería poder {nameof(Read)} alguna cosa?\r\n" +
			" Si";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.True(ide.Can(Read).Something(), permissionExplanation + query);
	}
	[Fact(DisplayName = "Rol - Cannot admin something")]
	public void CannotAdminSomething()
	{
		var pro = services.BuildServiceProvider();
		var ide = new IdentityDao("test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Edit.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", pro.GetRequiredService<TypeDiscriminatorFactory>().FromType<CategoryDao>(true), ScopePropagation.ToMe | ScopePropagation.ToInclusions)
				}
			},
			new PermissionDao("", "", ide, Read.Id.ToString() ?? "") {
				Value = false
			}
		};
		var permissionExplanation = "Tengo:\r\n" +
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
		var ide = new IdentityDao("test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Edit.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", Categories.Purchases, ScopePropagation.ToMe | ScopePropagation.ToExclusions)
				}
			}
		};
		var permissionExplanation = "Tengo:\r\n" +
			$" - Concedido el permiso para {nameof(Edit)} en {nameof(Categories.Purchases)}\r\n";
		var query =
			$"¿Debería poder {nameof(Delete)} en {nameof(Categories.Purchases)}?\r\n" +
			" No";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.False(ide.Can(Delete).Instance(Categories.Purchases), permissionExplanation + query);
		query =
			$"¿Debería poder {nameof(Read)} en {nameof(Categories.Purchases)}?\r\n" +
			" Si";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.True(ide.Can(Read).Instance(Categories.Purchases), permissionExplanation + query);
		query =
			$"¿Debería poder {nameof(Delete)} en {nameof(Categories.Purchases)}?\r\n" +
			" No";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.False(ide.Can(Delete).Type<WordDocumentDao>(), permissionExplanation + query);
		query =
			$"¿Debería poder {nameof(Read)} en {nameof(Categories.Purchases)}?\r\n" +
			" Si";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.True(ide.Can(Read).Type<WordDocumentDao>(), permissionExplanation + query);
	}
	[Fact(DisplayName = "Rol - Cannot by instance")]
	public void CannotByInstance()
	{
		var ide = new IdentityDao("test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Edit.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", Categories.Purchases, ScopePropagation.ToMe)
				}
			}
		};
		var permissionExplanation = "Tengo:\r\n" +
			$" - Concedido el permiso para {nameof(Edit)} en {nameof(Categories.Purchases)}\r\n";
		var query =
			$"¿Debería poder {nameof(Edit)} una instancia de documento de Word sin categoria?\r\n" +
			" No";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.False(ide.Can(Edit).Instance(Documents.Word1), permissionExplanation + query);
	}
	[Fact(DisplayName = "Rol - Cannot by type")]
	public void CannotByType()
	{
		var ide = new IdentityDao("test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Edit.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", Cities.SanFrancisco, ScopePropagation.ToMe)
				}
			},
			new PermissionDao("", "", ide, Edit.Id.ToString() ?? "") {
				Value = false,
				Scopes = new[] {
					new ScopeDao("", "", States.California, ScopePropagation.ToMe | ScopePropagation.ToInclusions)
				}
			}
		}.ToList();
		var permissionExplanation = "Tengo:\r\n" +
			$" - Concedido el permiso para {nameof(Edit)} en {nameof(Cities.SanFrancisco)}\r\n" +
			$" - Denegado el permiso para {nameof(Edit)} en {nameof(States.California)} y sus hijos\r\n";
		var query =
			"¿Debería ser root?\r\n" +
			" No";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.False(ide.IsRoot(), permissionExplanation + query);
		query =
			$"¿Debería poder {nameof(Edit)} en {nameof(Cities.SanFrancisco)}?\r\n" +
			" No";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.False(ide.Can(Edit).Instance(Cities.SanFrancisco), permissionExplanation + query);
	}
	[Fact(DisplayName = "Rol - Cannot for different discriminator")]
	public void CannotForDifferent()
	{
		var pro = services.BuildServiceProvider();
		var ide = new IdentityDao("test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Edit.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", pro.GetRequiredService<TypeDiscriminatorFactory>().FromType<CategoryDao>(true), ScopePropagation.ToMe | ScopePropagation.ToInclusions)
				}
			}
		};
		var permissionExplanation = "Tengo:\r\n" +
			$" - Concedido el permiso para {nameof(Edit)} el tipo {nameof(CategoryDao)} y derivados\r\n";
		var query =
			$"¿Debería poder {nameof(Read)} en {nameof(Cities.Buffalo)}?\r\n" +
			" No";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.False(ide.Can(Read).Instance(Cities.Buffalo), permissionExplanation + query);
	}
	[Fact(DisplayName = "Rol - Cannot for different discriminator2")]
	public void CannotForDifferent2()
	{
		var pro = services.BuildServiceProvider();
		var ide = new IdentityDao("test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Edit.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", pro.GetRequiredService<TypeDiscriminatorFactory>().FromType<CategoryDao>(true), ScopePropagation.ToMe | ScopePropagation.ToInclusions)
				}
			},
			new PermissionDao("", "", ide, Read.Id.ToString() ?? "") {
				Value = false,
				Scopes = new[] {
					new ScopeDao("", "", pro.GetRequiredService<TypeDiscriminatorFactory>().FromType<CategoryDao>(true), ScopePropagation.ToMe | ScopePropagation.ToInclusions)
				}
			}
		};
		var permissionExplanation = "Tengo:\r\n" +
			$" - Concedido el permiso para {nameof(Edit)} el tipo {nameof(CategoryDao)} y derivados\r\n" +
			$" - Denegado el permiso para {nameof(Read)} el tipo {nameof(CategoryDao)} y derivados\r\n";
		var query =
			$"¿Debería poder {nameof(Read)} la categoria '{nameof(Categories.Purchases)}'?\r\n" +
			" No";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.False(ide.Can(Read).Instance(Categories.Purchases), permissionExplanation + query);
	}
	[Fact(DisplayName = "Rol - Empty discriminator")]
	public void EmptyDiscriminator()
	{
		var pro = services.BuildServiceProvider();
		var ide = new IdentityDao("test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Admin.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", Categories.Purchases, ScopePropagation.ToMe | ScopePropagation.ToInclusions)
				}
			}
		}.ToList();
		var permissionExplanation = "Tengo:\r\n" +
			" - Permiso ADMIN en la categoria 'Purchases' y sus subcategorias\r\n";
		var query =
			$"¿Debería poder '{nameof(Create)}' una instancia de documento Word sin categoria?\r\n" +
			" No";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.False(ide.Can(Create).Instance(Documents.Word1), permissionExplanation + query);
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.False(ide.Can(Create).ByAll(
			pro.GetRequiredService<TypeDiscriminatorFactory>().FromType<WordDocumentDao>(true),
			Discriminator.Empty<CategoryDao>()), permissionExplanation + query);
		query =
			$"¿Debería poder '{nameof(Create)}' una instancia de documento Word de la categoria 'Purchases'?\r\n" +
			" Si";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.True(ide.Can(Create).Instance(Documents.Word1.Transform(w => w.Category = Categories.Purchases)), permissionExplanation + query);
		query =
			$"¿Debería poder '{nameof(Create)}' una instancia de documento Word de la categoria 'Sales'?\r\n" +
			" No";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.False(ide.Can(Create).Instance(Documents.Word1.Transform(w => w.Category = Categories.Sales)), permissionExplanation + query);
	}
	[Fact(DisplayName = "Rol - Grant for discriminator and denied for other discriminator")]
	public void GrantForDiscriminatorAndDeniedOther()
	{
		var pro = services.BuildServiceProvider();
		var ide = new IdentityDao("Test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Manage.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", pro.GetRequiredService<TypeDiscriminatorFactory>().FromType<FileDao>(true), ScopePropagation.ToMe | ScopePropagation.ToInclusions)
				}
			},
			new PermissionDao("", "", ide, Read.Id.ToString() ?? "") {
				Value = false,
				Scopes = new[] {
					new ScopeDao("", "", Countries.Spain, ScopePropagation.ToMe | ScopePropagation.ToInclusions)
					// Si agrego este segundo discriminador, pasa el TEST.
					// La cosa es, cuando tenemos un permiso discriminado por un tipo de discriminador y en la query tengo otro tipo de discriminador .. pasa el permiso el filtro?
					//new ScopeDao
					//{
					//    Discriminator = TypeDiscriminator.Empty,
					//    Propagation = ScopePropagation.ToMe| ScopePropagation.ToInclusions
					//},
				}
			}
		};
		var permissionExplanation = "Tengo:\r\n" +
			" - Permiso MANAGE para el tipo 'File' y sus derivados\r\n" +
			" - Denegado READ en el pais 'Spain' y sus sublocalizaciones\r\n";
		var query =
			$"¿Debería poder '{nameof(Read)}' una instancia del tipo '{nameof(WordDocumentDao)}' sin categoria?\r\n" +
			" Si";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.True(ide.Can(Read).Instance(Documents.Word1), permissionExplanation + query);
		query =
			$"¿Debería poder '{nameof(Read)}' el tipo '{nameof(DocumentDao)}'?\r\n" +
			" Si";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.True(ide.Can(Read).Type<DocumentDao>(), permissionExplanation + query);
		query =
			$"¿Debería poder '{nameof(Read)}' el tipo '{Helpers.TypeDiscriminatorIds.OfficeDocument}'?\r\n" +
			" Si";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.True(ide.Can(Read).ByAll(pro.GetRequiredService<TypeDiscriminatorFactory>().FromId(Helpers.TypeDiscriminatorIds.OfficeDocument)), permissionExplanation + query);
	}
	[Fact(DisplayName = "Rol - Grant for discriminator and denied for children")]
	public void GrantParentDeniedChildren()
	{
		var pro = services.BuildServiceProvider();
		var ide = new IdentityDao("test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Admin.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", Countries.Spain, ScopePropagation.ToMe | ScopePropagation.ToInclusions)
				}
			},
			new PermissionDao("", "", ide, Read.Id.ToString() ?? "") {
				Value = false,
				Scopes = new[] {
					new ScopeDao("", "", Cities.Madrid, ScopePropagation.ToMe | ScopePropagation.ToInclusions),
					new ScopeDao("", "", pro.GetRequiredService<TypeDiscriminatorFactory>().FromType<PersonDao>(true), ScopePropagation.ToMe | ScopePropagation.ToInclusions)
				}
			}
		};
		var permissionExplanation = "Tengo:\r\n" +
			" - Permiso ADMIN en el estado 'Spain' y sus sublocalizaciones\r\n" +
			" - Denegado READ personas en la ciudad 'Madrid' y sus sublocalizaciones\r\n";
		var query =
			$"¿Debería poder '{nameof(Create)}' una instancia de persona sin ciudad?\r\n" +
			" No";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.False(ide.Can(Create).Instance(Persons.Admin), permissionExplanation + query);
		query =
			$"¿Debería poder '{nameof(Create)}' una instancia de persona sin ciudad?\r\n" +
			" No";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.False(ide.Can(Create).ByAll(
			pro.GetRequiredService<TypeDiscriminatorFactory>().FromType<PersonDao>(true),
			Discriminator.Empty<CityDao>()), permissionExplanation + query);
		{
			query =
				$"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Madrid?\r\n" +
				" No";
			PrintTestTriedStarted(permissionExplanation + query);
			Assert.False(ide.Can(Create).Instance(Persons.MadridAdmin), permissionExplanation + query);
			PrintTestTriedStarted(permissionExplanation + query);
			Assert.False(ide.Can(Create).ByAll(
					pro.GetRequiredService<TypeDiscriminatorFactory>().FromType<PersonDao>(true),
					Cities.Madrid),
				permissionExplanation + query);
		}
		{
			query =
				$"¿Debería poder '{nameof(Create)}' una instancia de persona con la ciudad Alcorcon?\r\n" +
				" Si";
			PrintTestTriedStarted(permissionExplanation + query);
			Assert.True(ide.Can(Create).Instance(Persons.AlcorconAdmin), permissionExplanation + query);
			PrintTestTriedStarted(permissionExplanation + query);
			Assert.True(ide.Can(Create).ByAll(
				pro.GetRequiredService<TypeDiscriminatorFactory>().FromType<PersonDao>(),
				Cities.Alcorcon), permissionExplanation + query);
		}
		var source = new[] { Persons.AlcorconAdmin, Persons.MadridAdmin };
		query =
			$"¿Debería filtrar para '{nameof(Create)}' una instancia de pesona con la ciudad Alcorcon?\r\n" +
			" No";
		PrintTestTriedStarted(permissionExplanation + query);
		var source1 = source.AuthorizedTo(ide, Create);
		using (Printer.Indent("Source filtered results:"))
			foreach (var per in source1)
				Printer.WriteLine("Person: " + per);
		Assert.True(source1.Contains(Persons.AlcorconAdmin), permissionExplanation + query);
		query =
			$"¿Debería filtrar para '{nameof(Create)}' una instancia de pesona con la ciudad Madrid?\r\n" +
			" Si";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.False(source.AuthorizedTo(ide, Create).Contains(Persons.MadridAdmin), permissionExplanation + query);
		query =
			$"¿Debería poder '{nameof(Read)}' objetos del tipo Word?\r\n" +
			" Si";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.True(ide.Can(Read).Type<WordDocumentDao>(), permissionExplanation + query);
	}
	[Fact(DisplayName = "Rol - No permissions")]
	public void NoPermissions()
	{
		var ide = new IdentityDao("test", "Test");
		var permissionExplanation = "Tengo:\r\n" +
			" - Ningún permiso\r\n";
		var query =
			"¿Debería ser root?\r\n" +
			" No";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.False(ide.IsRoot(), permissionExplanation + query);
		query =
			$"¿Debería poder '{nameof(Read)}' alguna cosa?\r\n" +
			" No";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.False(ide.Can(Read).Something(), permissionExplanation + query);
		query =
			$"¿Debería poder {nameof(Edit)} en {nameof(Cities.SanFrancisco)}?\r\n" +
			" No";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.False(ide.Can(Edit).Instance(Cities.SanFrancisco), permissionExplanation + query);
	}
	[Fact(DisplayName = "Rol - Only read permission")]
	public void OnlyReadPermission()
	{
		var ide = new IdentityDao("test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Read.Id.ToString() ?? "") {
				Value = true
			}
		}.ToList();
		var permissionExplanation = "Tengo:\r\n" +
			" - Permiso ONLY READ\r\n";
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
	[Fact(DisplayName = "Rol - Root permission")]
	public void RootPermission()
	{
		var ide = new IdentityDao("test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Admin.Id.ToString() ?? "") {
				Value = true
			}
		}.ToList();
		var permissionExplanation = "Tengo:\r\n" +
			" - Permiso ROOT\r\n";
		var query =
			"¿Debería ser root?\r\n" +
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
			$"¿Debería poder '{nameof(Edit)}' en '{nameof(Documents.Word1)}'?\r\n" +
			" Si";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.True(ide.Can(Edit).Instance(Documents.Word1), permissionExplanation + query);
		query =
			$"¿Debería poder '{nameof(Edit)}' las '{nameof(CityDao)}'?\r\n" +
			" Si";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.True(ide.Can(Edit).Type<CityDao>(), permissionExplanation + query);
	}
	[Fact(DisplayName = "Rol - Type discriminator related to any permission scope")]
	public void TypeDiscriminatorRelatedToAnyPermissionScope()
	{
		var pro = services.BuildServiceProvider();
		var ide = new IdentityDao("test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Admin.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", pro.GetRequiredService<TypeDiscriminatorFactory>().FromType<RolDao>(true), ScopePropagation.ToMe | ScopePropagation.ToInclusions)
				}
			},
			new PermissionDao("", "", ide, Admin.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", pro.GetRequiredService<TypeDiscriminatorFactory>().FromType<CityDao>(true), ScopePropagation.ToMe | ScopePropagation.ToInclusions)
				}
			}
		}.ToList();
		var permissionExplanation = "Tengo:\r\n" +
			" - Permiso ADMIN para el tipo 'Rol' y sus derivados\r\n" +
			" - Permiso ADMIN para el tipo 'City' y sus derivados\r\n";
		var query =
			$"¿Debería poder '{nameof(Admin)}' la instancia '{nameof(Identities.Customer)}' del tipo '{nameof(RolDao)}'?\r\n" +
			" Si";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.True(ide.Can(Admin).Instance(Identities.Customer), permissionExplanation + query);

		//query =
		//    $"¿Debería poder '{nameof(Create)}' las '{nameof(CityDao)}'?\r\n" +
		//    " No";
		//PrintTestTriedStarted(permissionExplanation + query);
		//Assert.False(ide.Can(Create).Type<CityDao>(), permissionExplanation + query);
	}
	[Fact(DisplayName = "Rol - Type not discriminated")]
	public void TypeNotDiscriminated()
	{
		var ide = new IdentityDao("test", "Test");
		ide.Permissions = new[] {
			new PermissionDao("", "", ide, Read.Id.ToString() ?? "") {
				Value = true
			}
		}.ToList();
		var permissionExplanation = "Tengo:\r\n" +
			" - Permiso ONLY READ\r\n";
		var query =
			$"¿Debería poder '{nameof(Admin)}' los '{nameof(PackageDao)}' (estan deshabilitados)?\r\n" +
			" Si";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.True(ide.Can(Admin).Type<PackageDao>(), permissionExplanation + query);
		Assert.True(ide.Can(Admin).Instance(Packages.Package1), permissionExplanation + query);
		Assert.True(new[] { Packages.Package1 }.AuthorizedTo(ide, Admin).Any(), permissionExplanation + query);
	}
	[Fact(DisplayName = "Rol - Unknown type")]
	public void UnknownType()
	{
		var ide = new IdentityDao("test", "Test");
		var permissionExplanation = "Tengo:\r\n" +
			" - Ningún permiso\r\n";
		var query =
			$"¿Debería poder '{nameof(Edit)}' del tipo '{nameof(PermissionDao)}'?\r\n" +
			" Si";
		PrintTestTriedStarted(permissionExplanation + query);
		Assert.True(ide.Can(Edit).Type<PermissionDao>(), permissionExplanation + query);
	}
}