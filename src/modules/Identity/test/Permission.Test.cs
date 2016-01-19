using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Fuxion.Identity.FunctionGraph;
namespace Fuxion.Identity.Test
{
    class Permission : IPermission
    {
        public Permission(bool value, IFunction function, params IScope[] scopes)
        {
            Value = value;
            //Rol = rol;
            Function = function;
            Scopes = scopes;
        }
        //public IRol Rol { get; set; }
        public IFunction Function { get; set; }
        public IEnumerable<IScope> Scopes { get; set; }
        public bool Value { get; set; }
    }
    [TestClass]
    public class PermissionTest
    {
        FunctionGraph functions = new FunctionGraph();
        [TestMethod]
        public void WhenPermission_MatchByFunction()
        {
            // Un permiso de concesion para editar algo implicará que tambien puedo leerlo
            Assert.IsTrue(new Permission(true, Edit, null).MatchByFunction(functions, Read));
            // Un permiso de denegación para leer algo implicará que tampoco puedo editarlo
            Assert.IsTrue(new Permission(false, Read, null).MatchByFunction(functions, Edit));
        }
        [TestMethod]
        public void WhenPermission_MatchByDiscriminatorsType()
        {
            var Department = Guid.NewGuid();
            var Location = Guid.NewGuid();
            var WorkerClass = Guid.NewGuid();

            Func<Guid> x = () => Guid.NewGuid(); // x() representa cualquier valor, no es relevante
            var depId = x();
            var locId = x();
            // CASE 1
            // Los discriminadores encajan exacatamente, el permiso cumple
            Assert.IsTrue(
                new Permission(true, Read,
                    // Yo tengo 'Department' y 'Location'
                    new Scope(new GuidDiscriminator(depId, "Sales", null, null, Department, "Department"), 0),
                    //new Scope(new GuidDiscriminator(x(), "", null, null, Department, "Department"), 0),
                    new Scope(new GuidDiscriminator(locId, "", null, null, Location, "Location"), 0)
                ).MatchByDiscriminatorsType(new[] {
                    // Me presentan 'Department' y 'Location'
                    new GuidDiscriminator(depId, "Sales", null,null,Department, "Department"),
                    new GuidDiscriminator( locId, "", null,null,Location, "Location")
                }));
            // CASE 2
            // Faltan discriminadores, se han presentado una serie de discriminadores, pero este permiso tiene mas, no cumple
            Assert.IsFalse(
                new Permission(true, Read,
                    // Yo tengo 'Department' y 'Location'
                    new Scope(new GuidDiscriminator(x(), "", null, null, Department, "Department" ), 0),
                    new Scope(new GuidDiscriminator(x(), "", null, null, Location, "Location"), 0)
                ).MatchByDiscriminatorsType(new[] {
                    // Me presentan 'Department'
                    new GuidDiscriminator(x(), "", null,null,Department, "Department"),
                }));
            // CASE 3
            // Sobran discriminadores, se han presentado más discriminadores que los que aplican en este permiso, se ignorarán,
            // el permiso cumple
            Assert.IsTrue(
                new Permission(true, Read,
                    // Yo tengo 'Department' y 'Location'
                    new Scope(new GuidDiscriminator(x(), "", null, null, Department, "Department"), 0),
                    new Scope(new GuidDiscriminator(x(), "", null, null, Location, "Location"), 0)
                ).MatchByDiscriminatorsType(new[] {
                    // Me presentan 'Department', 'Location' y 'WorkerClass'
                    new GuidDiscriminator(x(), "", null, null,Department, "Department"),
                    new GuidDiscriminator(x(), "", null, null,Location, "Location"),
                    new GuidDiscriminator(x(), "", null, null,WorkerClass, "WorkerClass"),
                }));
            // CASE 4
            // Permiso de Root, sin discriminadores
            Assert.IsTrue(
                new Permission(true, Read
                    // Yo no tengo nada
                    ).MatchByDiscriminatorsType(new[]
                    {
                        // Me presentan 'Department', 'Location' y 'WorkerClass'
                        new GuidDiscriminator(x(), "", null, null,Department, "Department"),
                        new GuidDiscriminator(x(), "", null, null,Location, "Location"),
                        new GuidDiscriminator(x(), "", null, null,WorkerClass, "WorkerClass"),
                    }));
        }
        [TestMethod]
        public void WhenPermission_MatchByDiscriminatorsPath()
        {
            //var LocationType = Guid.NewGuid();
            //var USA = Guid.NewGuid();
            //var California = Guid.NewGuid();
            //var SanFrancisco = Guid.NewGuid();

            //var CaliforniaDiscriminator = new GuidDiscriminator(California, "California", new[] { SanFrancisco }, new[] { USA }, LocationType, "LocationType");
            //var USADiscriminator = new GuidDiscriminator(USA, "USA", new Guid[] { California, SanFrancisco }, new Guid[] { }, LocationType, "LocationType");
            //var SanFranciscoDiscriminator = new GuidDiscriminator(SanFrancisco, "SanFrancisco", new Guid[] { }, new Guid[] { California, SanFrancisco }, LocationType, "LocationType");
            // CASE 1 - Propagation to parents
            var propagation = ScopePropagation.ToExclusions;
            Assert.IsFalse(
                new Permission(true, Read,
                    new Scope(CaliforniaDiscriminator, propagation)
                ).MatchByDiscriminatorsPath(new[] {
                    new GuidDiscriminator(California, "California", new[] { USA },new[] { SanFrancisco },LocationType, "LocationType")
                }), $"Permiso de {nameof(California)} hacia {propagation}, me pasan {nameof(California)}, no debería encajar");
            Assert.IsTrue(
                new Permission(true, Read,
                    new Scope(CaliforniaDiscriminator, propagation)
                ).MatchByDiscriminatorsPath(new[] {
                    USADiscriminator
                }), $"Permiso de {nameof(California)} hacia {propagation}, me pasan {nameof(USA)}, debería encajar");
            Assert.IsFalse(
                new Permission(true, Read,
                    new Scope(CaliforniaDiscriminator, propagation)
                ).MatchByDiscriminatorsPath(new[] {
                    SanFranciscoDiscriminator
                }), $"Permiso de {nameof(California)} hacia {propagation}, me pasan {nameof(SanFrancisco)}, no debería encajar");

            // CASE 2 - Propagation to me
            propagation = ScopePropagation.ToMe;
            Assert.IsTrue(
                new Permission(true, Read,
                    new Scope(CaliforniaDiscriminator, propagation)
                ).MatchByDiscriminatorsPath(new[] {
                    CaliforniaDiscriminator
                }), $"Permiso de {nameof(California)} hacia {propagation}, me pasan {nameof(California)}, debería encajar");
            Assert.IsFalse(
                new Permission(true, Read,
                    new Scope(CaliforniaDiscriminator, propagation)
                ).MatchByDiscriminatorsPath(new[] {
                    USADiscriminator
                }), $"Permiso de {nameof(California)} hacia {propagation}, me pasan {nameof(USA)}, no debería encajar");
            Assert.IsFalse(
                new Permission(true, Read,
                    new Scope(CaliforniaDiscriminator, propagation)
                ).MatchByDiscriminatorsPath(new[] {
                    SanFranciscoDiscriminator
                }), $"Permiso de {nameof(California)} hacia {propagation}, me pasan {nameof(SanFrancisco)}, no debería encajar");

            // CASE 3 - Propagation to childs
            propagation = ScopePropagation.ToInclusions;
            Assert.IsFalse(
                new Permission(true, Read,
                    new Scope(CaliforniaDiscriminator, propagation)
                ).MatchByDiscriminatorsPath(new[] {
                    CaliforniaDiscriminator
                }), $"Permiso de {nameof(California)} hacia {propagation}, me pasan {nameof(California)}, no debería encajar");
            Assert.IsFalse(
                new Permission(true, Read,
                    new Scope(CaliforniaDiscriminator, propagation)
                ).MatchByDiscriminatorsPath(new[] {
                    USADiscriminator
                }), $"Permiso de {nameof(California)} hacia {propagation}, me pasan {nameof(USA)}, no debería encajar");
            Assert.IsTrue(
                new Permission(true, Read,
                    new Scope(CaliforniaDiscriminator, propagation)
                ).MatchByDiscriminatorsPath(new[] {
                    SanFranciscoDiscriminator
                }), $"Permiso de {nameof(California)} hacia {propagation}, me pasan {nameof(SanFrancisco)}, debería encajar");
        }
        static Guid LocationType = Guid.NewGuid();
        static Guid USA = Guid.NewGuid();
        static Guid California = Guid.NewGuid();
        static Guid SanFrancisco = Guid.NewGuid();
        GuidDiscriminator CaliforniaDiscriminator = new GuidDiscriminator(California, "California", new[] { SanFrancisco }, new[] { USA }, LocationType, "LocationType");
        GuidDiscriminator USADiscriminator = new GuidDiscriminator(USA, "USA", new Guid[] { California, SanFrancisco }, new Guid[] { }, LocationType, "LocationType");
        GuidDiscriminator SanFranciscoDiscriminator = new GuidDiscriminator(SanFrancisco, "SanFrancisco", new Guid[] { }, new Guid[] { California, SanFrancisco }, LocationType, "LocationType");
        [TestMethod]
        public void WhenPermission_Match()
        {
            //var LocationType = Guid.NewGuid();
            //var USA = Guid.NewGuid();
            //var USAPath = new Guid[] { };
            //var California = Guid.NewGuid();
            //var CaliforniaPath = new[] { USA };
            //var SanFrancisco = Guid.NewGuid();
            //var SanFranciscoPath = new[] { USA, California };

            //var Department = Guid.NewGuid();
            //var Sales = Guid.NewGuid();
            //var TI = Guid.NewGuid();
            //var Financial = Guid.NewGuid();

            var propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions;
            Assert.IsTrue(
                new Permission(true, Edit,
                    new Scope(
                        CaliforniaDiscriminator,
                        propagation)
                ).Match(functions, Read,
                    new[] {
                        SanFranciscoDiscriminator
                    }),
                $"Si tengo permiso para {nameof(Edit)} en {nameof(California)} y se propaga {propagation}." +
                $"¿Debería poder {nameof(Read)} en {nameof(SanFrancisco)}? => SI");

            Assert.IsTrue(
                new Permission(true, Edit,
                    new Scope(
                        CaliforniaDiscriminator, 
                        propagation)
                ).Match(functions, Read,
                    new[] {
                        SanFranciscoDiscriminator
                    }), 
                $"Si tengo permiso para {nameof(Edit)} en {nameof(California)} y se propaga {propagation}." +
                $"¿Debería poder {nameof(Read)} en {nameof(SanFrancisco)}? => SI");
        }
        [TestMethod]
        public void WhenPermission_Match2()
        {
            var LocationType ="LocationType";
            var USA = "USA";
            
            var California = "California";
            
            
            var SanFrancisco = "SanFrancisco";
            

            var USAIncludes = new string[] { California, SanFrancisco };
            var USAExcludes = new string[] { };
            var CaliforniIncludes = new[] { SanFrancisco };
            var CaliforniExcludes = new[] { USA };
            var SanFranciscIncludes = new string[] { };
            var SanFranciscExcludes = new[] { USA, California }; 

            var Department = "Department";
            var Sales = "Sales";
            var TI = "TI";
            var Financial = "Financial";

            var propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions;
            Assert.IsTrue(
                new Permission(true, Edit,
                    new Scope(
                        new StringDiscriminator(California, CaliforniIncludes, CaliforniExcludes, LocationType),
                        propagation)
                ).Match(new FunctionGraph(), Read,
                    new[] {
                        new StringDiscriminator(SanFrancisco, SanFranciscIncludes, SanFranciscExcludes,LocationType)
                    }),
                $"Si tengo permiso para {nameof(Edit)} en {nameof(California)} y se propaga {propagation}." +
                $"¿Debería poder {nameof(Read)} en {nameof(SanFrancisco)}? => SI");

            Assert.IsTrue(
                new Permission(true, Edit,
                    new Scope(
                        new StringDiscriminator(California, CaliforniIncludes, CaliforniExcludes, LocationType),
                        propagation)
                ).Match(new FunctionGraph(), Read,
                    new[] {
                        new StringDiscriminator(SanFrancisco, SanFranciscIncludes, SanFranciscExcludes,LocationType)
                    }),
                $"Si tengo permiso para {nameof(Edit)} en {nameof(California)} y se propaga {propagation}." +
                $"¿Debería poder {nameof(Read)} en {nameof(SanFrancisco)}? => SI");
        }
    }
}
