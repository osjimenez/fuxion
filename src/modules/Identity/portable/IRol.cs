using System;
using System.Collections.Generic;
using Fuxion.Identity.Helpers;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace Fuxion.Identity
{
    public interface IRol
    {
        string Name { get; }
        IEnumerable<IGroup> Groups { get; }
        IEnumerable<IPermission> Permissions { get; }
    }
    public static class RolExtensions
    {
        public class DiscriminatorCheck
        {
            public DiscriminatorCheck(string discriminator, IEnumerable<string> ids)
            {
                Discriminator = discriminator;
                Ids = ids;
            }
            public string Discriminator { get; private set; }
            public IEnumerable<string> Ids { get; private set; }
        }

        public class DiscriminatorCheckCollection
        {
            public DiscriminatorCheckCollection(bool value, IEnumerable<DiscriminatorCheck> checks)
            {
                Value = value;
                Checks = checks;
            }
            public bool Value { get; private set; }
            public IEnumerable<DiscriminatorCheck> Checks { get; private set; }
            public override string ToString()
            {
                return "Value:" + Value + " , Checks:" + Checks
                    .Aggregate("", (s, a) => s + a.Discriminator + "[" + a.Ids
                        .Aggregate("", (s2, a2) => s2 + a2 + ",", s2 => s2.Trim(',')) + "],", s => s.Trim(','));
            }
        }
        private static void PrintPermissions(string label, IEnumerable<IPermission> permissions, int ident = 0)
        {
            Debug.WriteLine(label + ":");
            foreach (var per in permissions)
            {
                Debug.WriteLine("".PadRight(ident, ' ') +
                                //per.Rol.PadRight(permissions.Max(p => p.Rol.Length), ' ') + " , f:" +
                                per.Function.Name.PadRight(permissions.Max(p => p.Function.Name.Length), ' ') + " , v:" +
                                per.Value + "".PadRight(per.Value ? 1 : 0, ' ') + " , ss:[" +
                                per.Scopes.Aggregate("", (str, actual) => str + actual + ",", str => str.Trim(',')) +
                                "]");
            }
        }
        public static IEnumerable<DiscriminatorCheckCollection> GetDiscriminators(this IRol me, IFunction function, params IDiscriminator[] discriminators)
        {
            var pers = me.GetPermissions(function, discriminators);
            //var pers = GetPermissions(IdentityMembership, function, discriminators);

            var granted = pers.Where(p => p.Value);
            var denied = pers.Where(p => !p.Value);
            Debug.WriteLine("Granted discriminators");
            foreach (var dis in granted)
                Debug.WriteLine(" - " + dis);
            Debug.WriteLine("Denied discriminators");
            foreach (var dis in denied)
                Debug.WriteLine(" - " + dis);

            //TODO - Oscar - Hay que desplegar los ids del discriminatorcheck para incluir los padres/hijos segun corresponda por su propagacion
            return granted.Select(p => new DiscriminatorCheckCollection(p.Value,
                p.Scopes.Select(s => new DiscriminatorCheck(s.Discriminator.TypeId.ToString(),
                        s.AllDiscriminators().Select(d=>d.Id.ToString())
                        //s.AllDiscriminators().Select(d => d.HierarchicalId.Split('/').Last())
                        )
                    )
                )
            );

        }
        ////        internal IEnumerable<PermissionClaim> GetPermissions(IAuthorizationRol rol, IAuthorizationFunction function, params IAuthorizationDiscriminator[] discriminators)
        internal static IEnumerable<IPermission> GetPermissions(this IRol me, IFunction function, params IDiscriminator[] discriminators)
        {
            //if (discriminators.Any(o => string.IsNullOrWhiteSpace(o.Path)))
            //    throw new ArgumentException("All discriminators must have Path so cannot be the root.");
            Debug.WriteLine("Filtering permissions:\r\n" +
                //                "   Rols => " + rol.AllMembership().Aggregate("", (s, a) => s + a.Id + " , ", s => s.Trim(',', ' ')) + "\r\n" +
                //"   Rols => " + membership.Aggregate("", (s, a) => s + a + " , ", s => s.Trim(',', ' ')) + "\r\n" +
                "   Functions => " + function.GetAllExclusions().Aggregate("", (s, a) => s + a.Id + " , ", s => s.Trim(',', ' ')) + "\r\n" +
                "   Discriminators => "
                + discriminators.Aggregate("", (str, actual) => str + actual.TypeId + "-" + actual.Inclusions + " , ", str => str.Trim(',', ' ')));
            Debug.WriteLine("");
            var permissions = me.Permissions.ToList().AsQueryable();
            PrintPermissions("Initial permissions", permissions, 3);
            #region Filter by rol
            //permissions = permissions.Where(p =>
            //    // Exclude permissions for roles that i don't take
            //    membership.Contains(p.Rol)
            //    );
            //PrintPermissions("After rol filters", permissions, 3);
            #endregion
            #region Filter by function
            permissions = permissions.Where(p =>
                // Exclude permissions granted with functions excluded by me
                (p.Value && function.GetAllExclusions().Contains(p.Function, new FunctionEqualityComparer()))
                    ||
                    // Or exclude permissions denied with functions included by me
                    (!p.Value && function.GetAllInclusions().Contains(p.Function, new FunctionEqualityComparer()))
                );
            PrintPermissions("After function filters", permissions, 3);
            #endregion
            #region Filter by discriminator
            /*
             * Casos:
             *  - Aspect not defined
             *	- Aspect defined as Root
             *	- Aspect not defined as Root
             */
            //permissions = permissions.Where(p =>
            //    // Exclude permissions in which some of my discriminators cannot be found
            //    (p.Value && function.AllExcludes().Select(f => f.Id).Contains(p.Function))
            //    ||
            //        // Or exclude permissions denied with functions included by me
            //    (!p.Value && function.AllIncludes().Select(f => f.Id).Contains(p.Function))
            //    );
            PrintPermissions("After discriminator Id filters", permissions, 3);
            #endregion
            #region Filter by Path
            var res = new List<IPermission>();
            foreach (var dis in discriminators)
            {
                var pers = permissions.Where(p => p.Scopes.Any(s => s.Discriminator.TypeId == dis.TypeId));
                res.AddRange(permissions.Except(pers));
                foreach (var per in permissions.Where(p => p.Scopes.Any(s => s.Discriminator.TypeId == dis.TypeId)))
                {
                    var sco = per.Scopes.Single(s => s.Discriminator.TypeId == dis.TypeId);

                    if (dis.GetAllInclusions().SequenceEqual(sco.Discriminator.GetAllInclusions()))
                    //if (dis.Path == sco.Discriminator.Path)
                    {
                        switch(sco.Propagation)
                        {
                            case ScopePropagation.ToMe:
                            case ScopePropagation.ToInclusions:
                                res.Add(per);
                                break;
                            default:
                                break;
                        }
                        // My id is same of permission scope
                        //if (sco.PropagateToMe) res.Add(per);
                        //else if (sco.PropagateToChilds) res.Add(per);
                    }
                    else if(!sco.Discriminator.GetAllInclusions().Any() ||
                            dis.GetAllInclusions().All(i=> sco.Discriminator.GetAllInclusions().Contains(i)) ||
                            !dis.GetAllInclusions().SequenceEqual(sco.Discriminator.GetAllInclusions()))
                    //else if ((string.IsNullOrWhiteSpace(sco.DiscriminatorPath) ||
                    //            dis.Path.StartsWith(sco.DiscriminatorPath)) &&
                    //            dis.Path != sco.DiscriminatorPath)
                    {
                        if (sco.Propagation == ScopePropagation.ToInclusions) res.Add(per);
                        // My id is child of permission scope
                        //if (sco.PropagateToChilds) res.Add(per);
                    }
                    else if(sco.Discriminator.GetAllInclusions().All(i=>dis.GetAllInclusions().Contains(i)))
                    //else if (sco.DiscriminatorPath.StartsWith(dis.Path))
                    {
                        switch (sco.Propagation)
                        {
                            case ScopePropagation.ToMe:
                            case ScopePropagation.ToInclusions:
                            case ScopePropagation.ToExclusions:
                                res.Add(per);
                                break;
                            default:
                                break;
                        }
                        // My id is parent of permission scope
                        //if (sco.PropagateToParents) res.Add(per);
                        //else if (sco.PropagateToMe) res.Add(per);
                        //else if (sco.PropagateToChilds) res.Add(per);
                    }
                    else
                    {
                        permissions = permissions.Except(new[] { per });
                    }
                }
            }
            PrintPermissions("After discriminator Path filter", res, 3);
            #endregion
            return res;
        }
        internal static bool IsFunctionAssigned(this IRol me, IFunction function, IDiscriminator[] discriminators, Action<string, bool> console)
        {
            return SearchForDeniedPermissions(me,function,discriminators,console) == null;
        }
        private static IPermission[] SearchForDeniedPermissions(this IRol me, IFunction function, IDiscriminator[] discriminators, Action<string, bool> console)
        {
            Action<string, bool> con = (m, i) => { if (console != null) console(m, i); };
            con($"SearchForDeniedPermissions:", true);
            con($"---Rol: {me.Name}", true);
            con($"---Funcion: {function.Name}", true);
            con($"---Discriminadores: {discriminators.Aggregate("", (a, c) => $"{a}      {c.Name}\r\n")}", true);
            // Parameters validation
            if (!function.IsValid()) throw new InvalidStateException($"The '{nameof(function)}' parameter has an invalid state");
            if (discriminators == null || !discriminators.Any()) throw new ArgumentException($"The '{nameof(discriminators)}' pararameter cannot be null or empty", nameof(discriminators));
            var invalidDiscriminator = discriminators.FirstOrDefault(d => !d.IsValid());
            if (invalidDiscriminator != null) throw new InvalidStateException($"The '{invalidDiscriminator}' discriminator has an invalid state");

            Func<IRol, IEnumerable<IPermission>> getPers = null;
            getPers = new Func<IRol, IEnumerable<IPermission>>(rol =>
            {
                var r = new List<IPermission>();
                if (rol.Permissions != null) r.AddRange(rol.Permissions);
                if(rol.Groups != null) foreach (var gro in rol.Groups) r.AddRange(getPers(gro));
                return r;
            });
            var permissions = getPers(me);
            con($"---Tengo: {permissions.Count()} permisos", true);
            IPermission[] res = null;
            // First, must take only permissions that match with the function and discriminators
            if(permissions == null || !permissions.Any())
            {
                con($"---Resultado: NO VALIDO - El rol no tiene permisos definidos", true);
                res = new IPermission[] { };
                return res;
            }
            var matchs = permissions.Where(p => p.Match(function, discriminators, con)).ToList();
            con($"---Encajan: {matchs.Count()} permisos", true);
            // Now, let's go to check that does not match any denegation permission
            res = matchs.Where(p => !p.Value).ToArray();
            con($"---De denegación: {res.Count()} permisos", true);
            if (res.Any())
            {
                con($"---Resultado: NO VALIDO - Denegado por un permiso", true);
                return res;
            }
            // Now, let's go to check that match at least one grant permission
            if (!matchs.Any(p => p.Value))
            {
                con($"---Resultado: NO VALIDO - Denegado por no encontrar un permiso de concesión", true);
                res = new IPermission[] { };
                return res;
            }
            con($"Resultado: VALIDO", true);
            return null;
        }
        internal static void CheckFunctionAssigned(this IRol me, IFunction function, IDiscriminator[] discriminators, Action<string,bool> console)
        {
            var res = me.SearchForDeniedPermissions(function, discriminators, console);
            if (res != null)
            {
                if (res.Any())
                    throw new UnauthorizedAccessException($"The function {function.Name} was requested for these discriminators:"
                        + discriminators.Aggregate("", (a, n) => $"{a}\n - {n.TypeName} = {n.Name}")
                        + "\nAnd have been matched in:\n" + res.First());
                else
                    throw new UnauthorizedAccessException($"The function {function.Name} was requested for these discriminators:"
                        + discriminators.Aggregate("", (a, n) => $"{a}\r\n - {n.TypeName} = {n.Name}")
                        + "\nAnd don't match any permission");
            }
        }
        public static IRolCan Can(this IRol me, params IFunction[] functions) { return new _RolCan(me, functions); }
        public static IRolFilter<T> Filter<T>(this IRol me, IQueryable<T> source) { return new _RolFilter<T>(me); }

        public static bool OfType<T>(this IRolCan me)
        {
            return OfAllTypes(me, typeof(T));
        }
        public static bool OfAllTypes<T1, T2>(this IRolCan me)
        {
            return OfAllTypes(me, typeof(T1), typeof(T2));
        }
        public static bool OfAllTypes<T1, T2, T3>(this IRolCan me)
        {
            return OfAllTypes(me, typeof(T1), typeof(T2), typeof(T3));
        }
        public static bool OfAllTypes(this IRolCan me, params Type[] types)
        {
            var r = me as _RolCan;
            var res = types.All(t => r.Rol.IsFunctionAssigned(
                r.Functions.First(),
                new[] { TypeDiscriminator.Create(t) },
                (m, _) => Debug.WriteLine(m)));
            return res;
        }
        public static bool This<T>(this IRolCan me, T value) { return false; }
        public static bool Any<T>(this IRolCan me, IEnumerable<T> value) { return false; }
        public static bool All<T>(this IRolCan me, IEnumerable<T> value) { return false; }

        public static IQueryable<T> For<T>(this IRolFilter<T> me, IFunction function) { return null; }
        public static IQueryable<T> ForAny<T>(this IRolFilter<T> me, IEnumerable<IFunction> functions) { return null; }
        public static IQueryable<T> ForAny<T>(this IRolFilter<T> me, params IFunction[] functions) { return null; }
        public static IQueryable<T> ForAll<T>(this IRolFilter<T> me, IEnumerable<IFunction> functions) { return null; }
        public static IQueryable<T> ForAll<T>(this IRolFilter<T> me, params IFunction[] functions) { return null; }

        //public static IQueryable<T> WhereCan<T>(this IQueryable<T> me, IFunction function) { return null; }
        public static IQueryable<T> WhereCan<T>(this IQueryable<T> me, params IFunction[] functions) { return null; }
        public static IQueryable<T> WhereCanAny<T>(this IQueryable<T> me, params IFunction[] functions) { return null; }
    }
    class _Discriminator : IDiscriminator
    {
        public IEnumerable<_Discriminator> Exclusions { get; set; }
        public object Id { get; set; }
        public IEnumerable<_Discriminator> Inclusions { get; set; }
        public string Name { get; set; }
        public object TypeId { get; set; }
        public string TypeName { get; set; }
        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions
        {
            get
            {
                return Exclusions;
            }
        }
        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions
        {
            get
            {
                return Inclusions;
            }
        }
    }
    public interface IRolCan { }
    class _RolCan : IRolCan
    {
        public _RolCan(IRol rol, IFunction[] functions) { Rol = rol; Functions = functions; }
        public IRol Rol { get; set; }
        public IFunction[] Functions { get; set; }
    }
    public interface IRolFilter<T> { }
    class _RolFilter<T> : IRolFilter<T>
    {
        public _RolFilter(IRol rol) { Rol = rol; }
        public IRol Rol { get; set; }
    }
}
