using System.Diagnostics;
using Fuxion.Identity.Test.Dao;
using Fuxion.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Identity.Test;

using static Functions;

public class PermissionTest : BaseTest<PermissionTest>
{
	public PermissionTest(ITestOutputHelper helper) : base(helper) => Context.Initialize();
	[Fact(DisplayName = "Permission - Match")]
	public void WhenPermission_Match()
	{
		var propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions;
		var per = new PermissionDao("", "", null!, Edit.Id.ToString() ?? "") {
			Value = true,
			Scopes = new ScopeDao[] {
				//new ScopeDao("","", States.California,propagation)
			}
		};
		var de = TypeDiscriminator.Empty;
		var dsf = Cities.SanFrancisco;
		var r = per.Match(false, Read, de, dsf);
		Debug.WriteLine("");
		Assert.True(new PermissionDao("", "", null!, Edit.Id.ToString() ?? "") {
				Value = true,
				Scopes = new ScopeDao[] {
					//new ScopeDao("","", States.California,propagation)
				}
			}.Match(false, Read, TypeDiscriminator.Empty, Cities.SanFrancisco),
			$"Si tengo permiso para {nameof(Edit)} en {nameof(States.California)} y se propaga {propagation}." + $"¿Debería poder {nameof(Read)} en {nameof(Cities.SanFrancisco)}? => SI");
		Assert.True(new PermissionDao("", "", null!, Edit.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", States.California, propagation)
				}
			}.Match(false, Read, TypeDiscriminator.Empty, Cities.SanFrancisco),
			$"Si tengo permiso para {nameof(Edit)} en {nameof(States.California)} y se propaga {propagation}." + $"¿Debería poder {nameof(Read)} en {nameof(Cities.SanFrancisco)}? => SI");
	}
	//[Fact(DisplayName = "Permission - Match by discriminators type")]
	//public void WhenPermission_MatchByDiscriminatorsType()
	//{
	//    // CASE 1
	//    // Los discriminadores encajan exacatamente, el permiso cumple
	//    Assert.True(
	//        new PermissionDao
	//        {
	//            Value = true,
	//            Function = Read.Id.ToString() ?? "" ?? "",
	//            Scopes = new[] {
	//                new ScopeDao { Discriminator = States.California },
	//                new ScopeDao { Discriminator = Discriminators.Category.Sales }
	//            }
	//        }.MatchByDiscriminatorsType2(new IDiscriminator[] {
	//            Discriminators.Category.Sales,
	//            States.California
	//        }));
	//    // CASE 2
	//    // Faltan discriminadores, se han presentado una serie de discriminadores, pero este permiso tiene mas, no cumple
	//    Assert.True(
	//        new PermissionDao
	//        {
	//            Value = true,
	//            Function = Read.Id.ToString() ?? "" ?? "",
	//            Scopes = new[] {
	//                new ScopeDao { Discriminator = States.California },
	//                new ScopeDao { Discriminator = Discriminators.Category.Sales }
	//            }
	//        }.MatchByDiscriminatorsType(new[] {
	//            Discriminators.Category.Sales,
	//        }));
	//    // CASE 3
	//    // Sobran discriminadores, se han presentado más discriminadores que los que aplican en este permiso, se ignorarán,
	//    // el permiso cumple
	//    Assert.True(
	//        new PermissionDao
	//        {
	//            Value = true,
	//            Function = Read.Id.ToString() ?? "" ?? "",
	//            Scopes = new[] {
	//                new ScopeDao { Discriminator = States.California },
	//                new ScopeDao { Discriminator = Discriminators.Category.Sales }
	//            }
	//        }.MatchByDiscriminatorsType(new IDiscriminator[] {
	//            Discriminators.Category.Sales,
	//            States.California,
	//            Discriminators.Tag.Urgent,
	//        }));
	//    // CASE 4
	//    // Permiso de Root, sin discriminadores
	//    Assert.True(
	//        new PermissionDao
	//        {
	//            Value = true,
	//            Function = Read.Id.ToString() ?? "" ?? "",
	//            Scopes = new ScopeDao[] {
	//                // Yo no tengo nada
	//            }
	//        }.MatchByDiscriminatorsType(new IDiscriminator[]
	//        {
	//            Discriminators.Category.Sales,
	//            States.California,
	//            Discriminators.Tag.Urgent,
	//        }));
	//}
	[Fact(DisplayName = "Permission - Match by discriminators path")]
	public void WhenPermission_MatchByDiscriminatorsPath()
	{
		// CASE 1 - Propagation to parents
		var propagation = ScopePropagation.ToExclusions;
		Assert.False(new PermissionDao("", "", null!, Read.Id.ToString() ?? "" ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", States.California, propagation)
				}
			}.MatchByDiscriminatorsInclusionsAndExclusions(false, TypeDiscriminator.Empty, States.California),
			$"Permiso de {nameof(States.California)} hacia {propagation}, me pasan {nameof(States.California)}, no debería encajar");
		Assert.True(new PermissionDao("", "", null!, Read.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", States.California, propagation)
				}
			}.MatchByDiscriminatorsInclusionsAndExclusions(false, TypeDiscriminator.Empty, Countries.Usa),
			$"Permiso de {nameof(States.California)} hacia {propagation}, me pasan {nameof(Countries.Usa)}, debería encajar");
		Assert.False(new PermissionDao("", "", null!, Read.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", States.California, propagation)
				}
			}.MatchByDiscriminatorsInclusionsAndExclusions(false, TypeDiscriminator.Empty, Cities.SanFrancisco),
			$"Permiso de {nameof(States.California)} hacia {propagation}, me pasan {nameof(Cities.SanFrancisco)}, no debería encajar");

		// CASE 2 - Propagation to me
		propagation = ScopePropagation.ToMe;
		Assert.True(new PermissionDao("", "", null!, Read.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", States.California, propagation)
				}
			}.MatchByDiscriminatorsInclusionsAndExclusions(false, TypeDiscriminator.Empty, States.California),
			$"Permiso de {nameof(States.California)} hacia {propagation}, me pasan {nameof(States.California)}, debería encajar");
		Assert.False(new PermissionDao("", "", null!, Read.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", States.California, propagation)
				}
			}.MatchByDiscriminatorsInclusionsAndExclusions(false, TypeDiscriminator.Empty, Countries.Usa),
			$"Permiso de {nameof(States.California)} hacia {propagation}, me pasan {nameof(Countries.Usa)}, no debería encajar");
		Assert.False(new PermissionDao("", "", null!, Read.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", States.California, propagation)
				}
			}.MatchByDiscriminatorsInclusionsAndExclusions(false, TypeDiscriminator.Empty, Cities.SanFrancisco),
			$"Permiso de {nameof(States.California)} hacia {propagation}, me pasan {nameof(Cities.SanFrancisco)}, no debería encajar");

		// CASE 3 - Propagation to childs
		propagation = ScopePropagation.ToInclusions;
		Assert.False(new PermissionDao("", "", null!, Read.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", States.California, propagation)
				}
			}.MatchByDiscriminatorsInclusionsAndExclusions(false, TypeDiscriminator.Empty, States.California),
			$"Permiso de {nameof(States.California)} hacia {propagation}, me pasan {nameof(States.California)}, no debería encajar");
		Assert.False(new PermissionDao("", "", null!, Read.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", States.California, propagation)
				}
			}.MatchByDiscriminatorsInclusionsAndExclusions(false, TypeDiscriminator.Empty, Countries.Usa),
			$"Permiso de {nameof(States.California)} hacia {propagation}, me pasan {nameof(Countries.Usa)}, no debería encajar");
		Assert.True(new PermissionDao("", "", null!, Read.Id.ToString() ?? "") {
				Value = true,
				Scopes = new[] {
					new ScopeDao("", "", States.California, propagation)
				}
			}.MatchByDiscriminatorsInclusionsAndExclusions(false, TypeDiscriminator.Empty, Cities.SanFrancisco),
			$"Permiso de {nameof(States.California)} hacia {propagation}, me pasan {nameof(Cities.SanFrancisco)}, debería encajar");
	}
	[Fact(DisplayName = "Permission - Match by function")]
	public void WhenPermission_MatchByFunction()
	{
		// Un permiso de concesion para editar algo implicará que tambien puedo leerlo
		Assert.True(new PermissionDao("", "", null!, Edit.Id.ToString() ?? "") {
			Value = true
		}.MatchByFunction(Read));
		// Un permiso de denegación para leer algo implicará que tampoco puedo editarlo
		Assert.True(new PermissionDao("", "", null!, Read.Id.ToString() ?? "" ?? "") {
			Value = false
		}.MatchByFunction(Edit));
	}
}