using System;
using System.Collections.Generic;
using static Fuxion.Identity.Functions;
using Fuxion.Identity.Test.Mocks;
using Fuxion.Identity.Test.Dao;
using static Fuxion.Identity.Test.Context;
using Xunit;
using Fuxion.Test;
using Xunit.Abstractions;

namespace Fuxion.Identity.Test
{
    public class PermissionTest : BaseTest
    {
        public PermissionTest(ITestOutputHelper helper) : base(helper) { }
        [Fact(DisplayName = "Permission - Match by function")]
        public void WhenPermission_MatchByFunction()
        {
            // Un permiso de concesion para editar algo implicará que tambien puedo leerlo
            Assert.True(new PermissionDao { Value = true, Function = Edit.Id.ToString() }.MatchByFunction(Read));
            // Un permiso de denegación para leer algo implicará que tampoco puedo editarlo
            Assert.True(new PermissionDao { Value = false, Function = Read.Id.ToString() }.MatchByFunction(Edit));
        }
        [Fact(DisplayName = "Permission - Match by discriminators type")]
        public void WhenPermission_MatchByDiscriminatorsType()
        {
            // CASE 1
            // Los discriminadores encajan exacatamente, el permiso cumple
            Assert.True(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminators.Location.State.California },
                        new ScopeDao { Discriminator = Discriminators.Category.Sales }
                    }
                }.MatchByDiscriminatorsType(new IDiscriminator[] {
                    Discriminators.Category.Sales,
                    Discriminators.Location.State.California
                }));
            // CASE 2
            // Faltan discriminadores, se han presentado una serie de discriminadores, pero este permiso tiene mas, no cumple
            Assert.True(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminators.Location.State.California },
                        new ScopeDao { Discriminator = Discriminators.Category.Sales }
                    }
                }.MatchByDiscriminatorsType(new[] {
                    Discriminators.Category.Sales,
                }));
            // CASE 3
            // Sobran discriminadores, se han presentado más discriminadores que los que aplican en este permiso, se ignorarán,
            // el permiso cumple
            Assert.True(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminators.Location.State.California },
                        new ScopeDao { Discriminator = Discriminators.Category.Sales }
                    }
                }.MatchByDiscriminatorsType(new IDiscriminator[] {
                    Discriminators.Category.Sales,
                    Discriminators.Location.State.California,
                    Discriminators.Tag.Urgent,
                }));
            // CASE 4
            // Permiso de Root, sin discriminadores
            Assert.True(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new ScopeDao[] {
                        // Yo no tengo nada
                    }
                }.MatchByDiscriminatorsType(new IDiscriminator[]
                {
                    Discriminators.Category.Sales,
                    Discriminators.Location.State.California,
                    Discriminators.Tag.Urgent,
                }));
        }
        [Fact(DisplayName = "Permission - Match by discriminators path")]
        public void WhenPermission_MatchByDiscriminatorsPath()
        {
            // CASE 1 - Propagation to parents
            var propagation = ScopePropagation.ToExclusions;
            Assert.False(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminators.Location.State.California, Propagation = propagation }
                    }
                }.MatchByDiscriminatorsInclusionsAndExclusions(new IDiscriminator[] {
                    Discriminators.Location.State.California
                }), $"Permiso de {nameof(Discriminators.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminators.Location.State.California)}, no debería encajar");
            Assert.True(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminators.Location.State.California, Propagation = propagation }
                    }
                }.MatchByDiscriminatorsInclusionsAndExclusions(new IDiscriminator[] {
                    Discriminators.Location.Country.Usa
                }), $"Permiso de {nameof(Discriminators.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminators.Location.Country.Usa)}, debería encajar");
            Assert.False(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminators.Location.State.California, Propagation = propagation}
                    }
                }.MatchByDiscriminatorsInclusionsAndExclusions(new IDiscriminator[] {
                    Discriminators.Location.City.SanFrancisco
                }), $"Permiso de {nameof(Discriminators.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminators.Location.City.SanFrancisco)}, no debería encajar");

            // CASE 2 - Propagation to me
            propagation = ScopePropagation.ToMe;
            Assert.True(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminators.Location.State.California, Propagation = propagation }
                    }
                }.MatchByDiscriminatorsInclusionsAndExclusions(new IDiscriminator[] {
                    Discriminators.Location.State.California
                }), $"Permiso de {nameof(Discriminators.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminators.Location.State.California)}, debería encajar");
            Assert.False(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminators.Location.State.California, Propagation = propagation }
                    }
                }.MatchByDiscriminatorsInclusionsAndExclusions(new[] {
                    Discriminators.Location.Country.Usa
                }), $"Permiso de {nameof(Discriminators.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminators.Location.Country.Usa)}, no debería encajar");
            Assert.False(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminators.Location.State.California, Propagation = propagation }
                    }
                }.MatchByDiscriminatorsInclusionsAndExclusions(new[] {
                    Discriminators.Location.City.SanFrancisco
                }), $"Permiso de {nameof(Discriminators.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminators.Location.City.SanFrancisco)}, no debería encajar");

            // CASE 3 - Propagation to childs
            propagation = ScopePropagation.ToInclusions;
            Assert.False(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminators.Location.State.California,  Propagation = propagation }
                    }
                }.MatchByDiscriminatorsInclusionsAndExclusions(new[] {
                    Discriminators.Location.State.California
                }), $"Permiso de {nameof(Discriminators.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminators.Location.State.California)}, no debería encajar");
            Assert.False(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao {Discriminator = Discriminators.Location.State.California, Propagation = propagation }
                    }
                }.MatchByDiscriminatorsInclusionsAndExclusions(new[] {
                    Discriminators.Location.Country.Usa
                }), $"Permiso de {nameof(Discriminators.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminators.Location.Country.Usa)}, no debería encajar");
            Assert.True(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminators.Location.State.California,  Propagation = propagation }
                    }
                }.MatchByDiscriminatorsInclusionsAndExclusions(new[] {
                    Discriminators.Location.City.SanFrancisco
                }), $"Permiso de {nameof(Discriminators.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminators.Location.City.SanFrancisco)}, debería encajar");
        }
        [Fact(DisplayName = "Permission - Match")]
        public void WhenPermission_Match()
        {
            var propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions;
            Assert.True(
                new PermissionDao
                {
                    Value = true,
                    Function = Edit.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminators.Location.State.California,Propagation = propagation }
                    }
                }.Match(Read,
                    new[] {
                        Discriminators.Location.City.SanFrancisco
                    }),
                $"Si tengo permiso para {nameof(Edit)} en {nameof(Discriminators.Location.State.California)} y se propaga {propagation}." +
                $"¿Debería poder {nameof(Read)} en {nameof(Discriminators.Location.City.SanFrancisco)}? => SI");

            Assert.True(
                new PermissionDao
                {
                    Value = true,
                    Function = Edit.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao {Discriminator = Discriminators.Location.State.California,Propagation = propagation}
                    }
                }.Match(Read,
                    new IDiscriminator[] {
                        Discriminators.Location.City.SanFrancisco
                    }),
                $"Si tengo permiso para {nameof(Edit)} en {nameof(Discriminators.Location.State.California)} y se propaga {propagation}." +
                $"¿Debería poder {nameof(Read)} en {nameof(Discriminators.Location.City.SanFrancisco)}? => SI");
        }
    }
}
