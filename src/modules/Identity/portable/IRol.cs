using System;
using System.Collections.Generic;
using Fuxion.Identity.Helpers;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Linq.Expressions;

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
        public static IEnumerable<IScope> GetScopes(this IRol me, IFunction[] functions, IDiscriminator[] discriminators)
        {
            IEnumerable<IScope> res = null;
            Printer.Ident($"GetScopes ...", () =>
            {
                var pers = me.GetPermissions(functions, discriminators);
                var granted = pers.Where(p => p.Value);
                var denied = pers.Where(p => !p.Value);
                Printer.Print("Granted permissions:");
                granted.Print(PrintMode.Table);
                Printer.Print($"Denied permissions");
                denied.Print(PrintMode.Table);
                res = pers.SelectMany(p => p.Scopes);
            });
            return res;
        }
        internal static IEnumerable<IPermission> GetPermissions(this IRol me, IFunction[] functions, IDiscriminator[] discriminators)
        {
            var permissions = me.AllPermissions();
            Printer.Ident($"{nameof(GetPermissions)} ...", () =>
            {
                var exclusions = functions.SelectMany(f => f.GetAllExclusions()).Distinct();
                var inclusions = functions.SelectMany(f => f.GetAllExclusions()).Distinct();
                Printer.Print("Functions: " + exclusions.Aggregate("", (s, a) => s + a.Id + " , ", s => s.Trim(',', ' ')));
                Printer.Ident("Discriminators:", () => discriminators.Print(PrintMode.Table));
                Printer.Ident("Initial permissions:", () => permissions.Print(PrintMode.Table));
                Printer.Print("");
                #region Filter by function
                Printer.Ident("After filter by function:", () =>
                {
                    permissions = permissions.Where(p =>
                        // Exclude permissions granted with functions excluded by me
                        (p.Value && exclusions.Contains(p.Function, new FunctionEqualityComparer()))
                            ||
                            // Or exclude permissions denied with functions included by me
                            (!p.Value && inclusions.Contains(p.Function, new FunctionEqualityComparer()))
                        );
                    permissions.Print(PrintMode.Table);
                });
                #endregion
                #region Filter by discriminator type
                Printer.Ident("After filter by discriminator type", () =>
                {
                    permissions.Print(PrintMode.Table);
                });

                #endregion
                #region Filter by discriminator propagation
                Printer.Ident("After filter by discriminator propagation", () =>
                {
                    var toExclude = new List<IPermission>();
                    foreach (var dis in discriminators)
                    {
                        var permissionsOfType = permissions.Where(p => p.Scopes.Any(s => s.Discriminator.TypeId == dis.TypeId) || !p.Scopes.Any());
                        Printer.Ident("permissionsOfType:", () => permissionsOfType.Print(PrintMode.Table));
                        //toExclude.AddRange(permissions.Except(permissionsOfType));
                        foreach (var per in permissions.Where(p => p.Scopes.Any(s => s.Discriminator.TypeId == dis.TypeId)))
                        {
                            var sco = per.Scopes.Single(s => s.Discriminator.TypeId == dis.TypeId);
                            if (dis.GetAllInclusions().SequenceEqual(sco.Discriminator.GetAllInclusions()))
                            {
                                switch (sco.Propagation)
                                {
                                    case ScopePropagation.ToMe:
                                    case ScopePropagation.ToInclusions:
                                        toExclude.Add(per);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else if (!sco.Discriminator.GetAllInclusions().Any() ||
                                    dis.GetAllInclusions().All(i => sco.Discriminator.GetAllInclusions().Contains(i)) ||
                                    !dis.GetAllInclusions().SequenceEqual(sco.Discriminator.GetAllInclusions()))
                            {
                                if (sco.Propagation == ScopePropagation.ToInclusions)
                                    toExclude.Add(per);
                            }
                            else if (sco.Discriminator.GetAllInclusions().All(i => dis.GetAllInclusions().Contains(i)))
                            {
                                switch (sco.Propagation)
                                {
                                    case ScopePropagation.ToMe:
                                    case ScopePropagation.ToInclusions:
                                    case ScopePropagation.ToExclusions:
                                        toExclude.Add(per);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                permissions = permissions.Except(new[] { per });
                            }
                        }
                    }
                    Printer.Ident("Permissions to exclude:", () => toExclude.Print(PrintMode.Table));
                    permissions = permissions.Except(toExclude, new PermissionEqualityComparer());
                    permissions.Print(PrintMode.Table);
                });
                #endregion
            });
            Printer.Ident("Returned permissions:", () => permissions.Print(PrintMode.Table));
            return permissions;
        }
        internal static Expression<Func<TEntity, bool>> FilterExpression<TEntity>(this IRol me, IFunction[] functions)
        {
            Expression<Func<TEntity, bool>> res = null;
            Printer.Ident($"{nameof(FilterExpression)} ...", () =>
            {
                Printer.Print("Type: " + typeof(TEntity).Name);
                var scopes = me.GetScopes(functions, new[] { TypeDiscriminator.Create(typeof(TEntity)) }).ToList();
                Printer.Print("Scopes:");
                scopes.Print(PrintMode.Table);
                var props = typeof(TEntity).GetRuntimeProperties()
                    .Where(p => p.GetCustomAttribute<DiscriminatedByAttribute>(true, false, false) != null)
                    .Select(p => new
                    {
                        PropertyInfo = p,
                        DiscriminatorTypeId =
                            p.GetCustomAttribute<DiscriminatedByAttribute>(true).Type.GetTypeInfo()
                                .GetCustomAttribute<DiscriminatorAttribute>(true).TypeId,
                    });
                Printer.Print("Properties => " + props.Aggregate("", (str, actual) => $"{str} {actual.PropertyInfo.Name} ({actual.PropertyInfo.PropertyType.GetSignature(false)}),", str => str.Trim(',', ' ')));
                Printer.Print("");
                Printer.Ident("Filter:", () =>
                {
                    Printer.Print("(");
                    for (int s = 0; s < scopes.Count(); s++)
                    {
                        var sco = scopes[s];
                        if (res != null) Printer.Print("AND(");
                        Expression<Func<TEntity, bool>> proExp = null;
                        Printer.Ident(() =>
                        {
                            var propsOfType = props.Where(p => p.DiscriminatorTypeId.Equals(sco.Discriminator.TypeId)).ToList();
                            for (int p = 0; p < propsOfType.Count; p++)
                            {
                                var pro = propsOfType[p].PropertyInfo;
                                Printer.Print("    " +
                                    sco.Discriminator.GetAllInclusions().Select(d => d.Id).Aggregate("",
                                        (s2, a) => s2 + pro.Name + " == " + a + " || ",
                                        s2 => s2.Trim('|', ' ')));

                                var castMethod = typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                                    .Where(m => m.Name == nameof(Enumerable.Cast))
                                                    .Single(m => m.GetParameters().Length == 1)
                                                    .MakeGenericMethod(pro.PropertyType);
                                var toListMethod = typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                                    .Where(m => m.Name == nameof(Enumerable.ToList))
                                                    .Single(m => m.GetParameters().Length == 1)
                                                    .MakeGenericMethod(pro.PropertyType);
                                var containsMethod = typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                                    .Where(m => m.Name == nameof(Enumerable.Contains))
                                                    .Single(m => m.GetParameters().Length == 2)
                                                    .MakeGenericMethod(pro.PropertyType);
                                var foreignKeys = sco.Discriminator.GetAllInclusions().Select(d => d.Id);
                                var foreignKeysCasted = castMethod.Invoke(null, new object[] { foreignKeys });
                                var foreignKeysListed = toListMethod.Invoke(null, new object[] { foreignKeysCasted });
                                var entityParameter = Expression.Parameter(typeof(TEntity));
                                var memberExpression = Expression.Property(entityParameter, pro);
                                var containsExpression = Expression.Call(containsMethod,
                                    Expression.Constant(foreignKeysListed, foreignKeysListed.GetType()),
                                    memberExpression);
                                var curExp = Expression.Lambda<Func<TEntity, bool>>(containsExpression, entityParameter);
                                if (
                                    (pro.PropertyType.GetTypeInfo().IsGenericType &&
                                     pro.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                                {
                                    var arg = Expression.Parameter(typeof(TEntity));
                                    var prop = Expression.Property(arg, pro);
                                    curExp = Expression.Lambda<Func<TEntity, bool>>(
                                        Expression.NotEqual(prop, Expression.Constant(null, prop.Type)),
                                        arg)
                                        .And(curExp);
                                }
                                proExp = (proExp == null ? curExp : proExp.Or(curExp));
                                if (proExp != null)
                                {
                                    if (p == propsOfType.Count - 1)
                                        Printer.Print(")");
                                    else
                                        Printer.Print(") OR (");
                                }
                            }
                        });
                        if (res != null) Printer.Print(")");
                        if (proExp != null)
                            res = (res == null ? proExp : res.And(proExp));
                    }
                });
                if (res == null && !scopes.Any())
                {
                    if (me.GetPermissions(functions, new[] { TypeDiscriminator.Create(typeof(TEntity)) }).Any())
                    {
                        res = Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(true), Expression.Parameter(typeof(TEntity)));
                    }
                    else {
                        res = Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(false), Expression.Parameter(typeof(TEntity)));
                    }
                }
                Printer.Print("Expression:" + (res == null ? "null" : res.ToString()));
            });
            return res;
        }
        private static IEnumerable<IPermission> AllPermissions(this IRol me)
        {
            var res = new List<IPermission>();
            res.AddRange(me.Permissions);
            foreach (var gro in me.Groups) res.AddRange(gro.AllPermissions());
            return res;
        }
        private static IEnumerable<IGroup> AllGroups(this IRol me)
        {
            var res = new List<IGroup>();
            res.AddRange(me.Groups);
            foreach (var gro in me.Groups) res.AddRange(gro.AllGroups());
            return res;
        }
        private static IPermission[] SearchForDeniedPermissions(this IRol me, IFunction function, IDiscriminator[] discriminators, Action<string, bool> console)
        {
            Action<string, bool> con = (m, i) => { if (console != null) console(m, i); };
            con($"SearchForDeniedPermissions:", true);
            con($"---Rol: {me.Name}", true);
            con($"---Funcion: {function.Name}", true);
            con($"---Discriminadores: {discriminators.Aggregate("", (a, c) => $"{a}      {c.Name}\r\n")}", true);
            // Validation
            if (!function.IsValid()) throw new InvalidStateException($"The '{nameof(function)}' parameter has an invalid state");
            if (discriminators == null || !discriminators.Any()) throw new ArgumentException($"The '{nameof(discriminators)}' pararameter cannot be null or empty", nameof(discriminators));
            var invalidDiscriminator = discriminators.FirstOrDefault(d => !d.IsValid());
            if (invalidDiscriminator != null) throw new InvalidStateException($"The '{invalidDiscriminator}' discriminator has an invalid state");

            Func<IRol, IEnumerable<IPermission>> getPers = null;
            getPers = new Func<IRol, IEnumerable<IPermission>>(rol =>
            {
                var r = new List<IPermission>();
                if (rol.Permissions != null) r.AddRange(rol.Permissions);
                if (rol.Groups != null) foreach (var gro in rol.Groups) r.AddRange(getPers(gro));
                return r;
            });
            var permissions = getPers(me);
            con($"---Tengo: {permissions.Count()} permisos", true);
            IPermission[] res = null;
            // First, must take only permissions that match with the function and discriminators
            if (permissions == null || !permissions.Any())
            {
                con($"---Resultado: NO VALIDO - El rol no tiene permisos definidos", true);
                res = new IPermission[] { };
                return res;
            }
            var matchs = permissions.Where(p => p.Match(function, discriminators)).ToList();
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
        internal static bool IsFunctionAssigned(this IRol me, IFunction function, IDiscriminator[] discriminators, Action<string, bool> console)
        {
            return SearchForDeniedPermissions(me, function, discriminators, console) == null;
        }
        //internal static void CheckFunctionAssigned(this IRol me, IFunction function, IDiscriminator[] discriminators, Action<string, bool> console)
        //{
        //    var res = me.SearchForDeniedPermissions(function, discriminators, console);
        //    if (res != null)
        //    {
        //        if (res.Any())
        //            throw new UnauthorizedAccessException($"The function {function.Name} was requested for these discriminators:"
        //                + discriminators.Aggregate("", (a, n) => $"{a}\n - {n.TypeName} = {n.Name}")
        //                + "\nAnd have been matched in:\n" + res.First());
        //        else
        //            throw new UnauthorizedAccessException($"The function {function.Name} was requested for these discriminators:"
        //                + discriminators.Aggregate("", (a, n) => $"{a}\r\n - {n.TypeName} = {n.Name}")
        //                + "\nAnd don't match any permission");
        //    }
        //}

        #region Fluent
        #region IRol
        public static IRolCan Can(this IRol me, params IFunction[] functions) { return new _RolCan(me, functions, false); }
        public static IRolEnsureCan EnsureCan(this IRol me, params IFunction[] functions) { return new _RolCan(me, functions, true); }
        #endregion
        #region Can().Type's
        // Only one type
        public static bool Type(this IRolCan me, Type type) { return CheckTypes((_RolCan)me, true, type); }
        public static void Type(this IRolEnsureCan me, Type type) { CheckTypes((_RolCan)me, true, type); }
        public static bool Type<T>(this IRolCan me) { return CheckTypes((_RolCan)me, true, typeof(T)); }
        public static void Type<T>(this IRolEnsureCan me) { CheckTypes((_RolCan)me, true, typeof(T)); }
        // Two types
        public static bool AllTypes<T1, T2>(this IRolCan me) { return CheckTypes((_RolCan)me, true, typeof(T1), typeof(T2)); }
        public static void AllTypes<T1, T2>(this IRolEnsureCan me) { CheckTypes((_RolCan)me, true, typeof(T1), typeof(T2)); }
        public static bool AnyType<T1, T2>(this IRolCan me) { return CheckTypes((_RolCan)me, false, typeof(T1), typeof(T2)); }
        public static void AnyType<T1, T2>(this IRolEnsureCan me) { CheckTypes((_RolCan)me, false, typeof(T1), typeof(T2)); }
        // Three types
        public static bool AllTypes<T1, T2, T3>(this IRolCan me) { return CheckTypes((_RolCan)me, true, typeof(T1), typeof(T2), typeof(T3)); }
        public static void AllTypes<T1, T2, T3>(this IRolEnsureCan me) { CheckTypes((_RolCan)me, true, typeof(T1), typeof(T2), typeof(T3)); }
        public static bool AnyType<T1, T2, T3>(this IRolCan me) { return CheckTypes((_RolCan)me, false, typeof(T1), typeof(T2), typeof(T3)); }
        public static void AnyType<T1, T2, T3>(this IRolEnsureCan me) { CheckTypes((_RolCan)me, false, typeof(T1), typeof(T2), typeof(T3)); }
        // Many types
        public static bool AllTypes(this IRolCan me, params Type[] types) { return CheckTypes((_RolCan)me, true, types); }
        public static void AllTypes(this IRolEnsureCan me, params Type[] types) { CheckTypes((_RolCan)me, true, types); }
        public static bool AnyType(this IRolCan me, params Type[] types) { return CheckTypes((_RolCan)me, false, types); }
        public static void AnyType(this IRolEnsureCan me, params Type[] types) { CheckTypes((_RolCan)me, false, types); }
        // -------------------------- IMPLEMENTATION
        private static bool CheckTypes(_RolCan me, bool forAll, params Type[] types)
        {
            var res = forAll
                ? types.All(t => me.Rol.IsFunctionAssigned(
                    me.Functions.First(),
                    new[] { TypeDiscriminator.Create(t) },
                    (m, _) => Debug.WriteLine(m)))
                : types.Any(t => me.Rol.IsFunctionAssigned(
                    me.Functions.First(),
                    new[] { TypeDiscriminator.Create(t) },
                    (m, _) => Debug.WriteLine(m)));
            if (me.ThrowExceptionIfCannot && !res)
                throw new UnauthorizedAccessException($"The rol '{me.Rol.Name}' cannot '{me.Functions.Aggregate("", (a, c) => a + c.Name + "·", a => a.Trim('·'))}' for the given types '{types}'");
            return res;
        }
        #endregion
        #region Can().Instance's
        // One instance
        public static bool Instance<T>(this IRolCan me, T value) { return CheckInstances((_RolCan)me, true, new[] { value }); }
        public static void Instance<T>(this IRolEnsureCan me, T value) { CheckInstances((_RolCan)me, true, new[] { value }); }
        // Many instances
        public static bool AllInstances<T>(this IRolCan me, params T[] values) { return CheckInstances((_RolCan)me, true, values); }
        public static void AllInstances<T>(this IRolEnsureCan me, params T[] values) { CheckInstances((_RolCan)me, true, values); }
        public static bool AnyInstance<T>(this IRolCan me, params T[] values) { return CheckInstances((_RolCan)me, false, values); }
        public static void AnyInstance<T>(this IRolEnsureCan me, params T[] values) { CheckInstances((_RolCan)me, false, values); }
        // -------------------------- IMPLEMENTATION
        private static bool CheckInstances<T>(_RolCan me, bool forAll, params T[] values)
        {
            me.Rol.FilterExpression<T>(me.Functions);
            var res = forAll ? values.AuthorizedTo(me.Functions).Count() == values.Count() : values.AuthorizedTo(me.Functions).Any();
            if (me.ThrowExceptionIfCannot && !res)
                throw new UnauthorizedAccessException($"The rol '{me.Rol.Name}' cannot '{me.Functions.Aggregate("", (a, c) => a + c.Name + "·", a => a.Trim('·'))}' for the given instances '{values}'");
            return res;
        }
        #endregion
        #endregion
    }
    public interface IRolCan { }
    public interface IRolEnsureCan { }
    class _RolCan : IRolCan, IRolEnsureCan
    {
        public _RolCan(IRol rol, IFunction[] functions, bool throwExceptionIfCannot)
        {
            Rol = rol;
            Functions = functions;
            ThrowExceptionIfCannot = throwExceptionIfCannot;
        }
        public IRol Rol { get; set; }
        public IFunction[] Functions { get; set; }
        public bool ThrowExceptionIfCannot { get; set; }
    }
}
