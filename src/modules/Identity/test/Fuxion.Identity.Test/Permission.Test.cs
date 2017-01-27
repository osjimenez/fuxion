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
                        new ScopeDao { Discriminator = Discriminator.Location.State.California },
                        new ScopeDao { Discriminator = Discriminator.Category.Sales }
                    }
                }.MatchByDiscriminatorsType(new IDiscriminator[] {
                    Discriminator.Category.Sales,
                    Discriminator.Location.State.California
                }));
            // CASE 2
            // Faltan discriminadores, se han presentado una serie de discriminadores, pero este permiso tiene mas, no cumple
            Assert.True(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminator.Location.State.California },
                        new ScopeDao { Discriminator = Discriminator.Category.Sales }
                    }
                }.MatchByDiscriminatorsType(new[] {
                    Discriminator.Category.Sales,
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
                        new ScopeDao { Discriminator = Discriminator.Location.State.California },
                        new ScopeDao { Discriminator = Discriminator.Category.Sales }
                    }
                }.MatchByDiscriminatorsType(new IDiscriminator[] {
                    Discriminator.Category.Sales,
                    Discriminator.Location.State.California,
                    Discriminator.Tag.Urgent,
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
                    Discriminator.Category.Sales,
                    Discriminator.Location.State.California,
                    Discriminator.Tag.Urgent,
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
                        new ScopeDao { Discriminator = Discriminator.Location.State.California, Propagation = propagation }
                    }
                }.MatchByDiscriminatorsPath(new IDiscriminator[] {
                    Discriminator.Location.State.California
                }), $"Permiso de {nameof(Discriminator.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminator.Location.State.California)}, no debería encajar");
            Assert.True(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminator.Location.State.California, Propagation = propagation }
                    }
                }.MatchByDiscriminatorsPath(new IDiscriminator[] {
                    Discriminator.Location.Country.Usa
                }), $"Permiso de {nameof(Discriminator.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminator.Location.Country.Usa)}, debería encajar");
            Assert.False(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminator.Location.State.California, Propagation = propagation}
                    }
                }.MatchByDiscriminatorsPath(new IDiscriminator[] {
                    Discriminator.Location.City.SanFrancisco
                }), $"Permiso de {nameof(Discriminator.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminator.Location.City.SanFrancisco)}, no debería encajar");

            // CASE 2 - Propagation to me
            propagation = ScopePropagation.ToMe;
            Assert.True(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminator.Location.State.California, Propagation = propagation }
                    }
                }.MatchByDiscriminatorsPath(new IDiscriminator[] {
                    Discriminator.Location.State.California
                }), $"Permiso de {nameof(Discriminator.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminator.Location.State.California)}, debería encajar");
            Assert.False(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminator.Location.State.California, Propagation = propagation }
                    }
                }.MatchByDiscriminatorsPath(new[] {
                    Discriminator.Location.Country.Usa
                }), $"Permiso de {nameof(Discriminator.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminator.Location.Country.Usa)}, no debería encajar");
            Assert.False(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminator.Location.State.California, Propagation = propagation }
                    }
                }.MatchByDiscriminatorsPath(new[] {
                    Discriminator.Location.City.SanFrancisco
                }), $"Permiso de {nameof(Discriminator.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminator.Location.City.SanFrancisco)}, no debería encajar");

            // CASE 3 - Propagation to childs
            propagation = ScopePropagation.ToInclusions;
            Assert.False(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminator.Location.State.California,  Propagation = propagation }
                    }
                }.MatchByDiscriminatorsPath(new[] {
                    Discriminator.Location.State.California
                }), $"Permiso de {nameof(Discriminator.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminator.Location.State.California)}, no debería encajar");
            Assert.False(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao {Discriminator = Discriminator.Location.State.California, Propagation = propagation }
                    }
                }.MatchByDiscriminatorsPath(new[] {
                    Discriminator.Location.Country.Usa
                }), $"Permiso de {nameof(Discriminator.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminator.Location.Country.Usa)}, no debería encajar");
            Assert.True(
                new PermissionDao
                {
                    Value = true,
                    Function = Read.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao { Discriminator = Discriminator.Location.State.California,  Propagation = propagation }
                    }
                }.MatchByDiscriminatorsPath(new[] {
                    Discriminator.Location.City.SanFrancisco
                }), $"Permiso de {nameof(Discriminator.Location.State.California)} hacia {propagation}, me pasan {nameof(Discriminator.Location.City.SanFrancisco)}, debería encajar");
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
                        new ScopeDao { Discriminator = Discriminator.Location.State.California,Propagation = propagation }
                    }
                }.Match(Read,
                    new[] {
                        Discriminator.Location.City.SanFrancisco
                    }),
                $"Si tengo permiso para {nameof(Edit)} en {nameof(Discriminator.Location.State.California)} y se propaga {propagation}." +
                $"¿Debería poder {nameof(Read)} en {nameof(Discriminator.Location.City.SanFrancisco)}? => SI");

            Assert.True(
                new PermissionDao
                {
                    Value = true,
                    Function = Edit.Id.ToString(),
                    Scopes = new[] {
                        new ScopeDao {Discriminator = Discriminator.Location.State.California,Propagation = propagation}
                    }
                }.Match(Read,
                    new IDiscriminator[] {
                        Discriminator.Location.City.SanFrancisco
                    }),
                $"Si tengo permiso para {nameof(Edit)} en {nameof(Discriminator.Location.State.California)} y se propaga {propagation}." +
                $"¿Debería poder {nameof(Read)} en {nameof(Discriminator.Location.City.SanFrancisco)}? => SI");
        }
    }
}
