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
                    var res = new List<IPermission>();
                    foreach (var dis in discriminators)
                    {
                        var pers = permissions.Where(p => p.Scopes.Any(s => s.Discriminator.TypeId == dis.TypeId) || !p.Scopes.Any());
                        Printer.Ident("pers:", () => pers.Print(PrintMode.Table));
                        res.AddRange(permissions.Except(pers));
                        foreach (var per in permissions.Where(p => p.Scopes.Any(s => s.Discriminator.TypeId == dis.TypeId)))
                        {
                            var sco = per.Scopes.Single(s => s.Discriminator.TypeId == dis.TypeId);
                            if (dis.GetAllInclusions().SequenceEqual(sco.Discriminator.GetAllInclusions()))
                            {
                                switch (sco.Propagation)
                                {
                                    case ScopePropagation.ToMe:
                                    case ScopePropagation.ToInclusions:
                                        res.Add(per);
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
                                    res.Add(per);
                            }
                            else if (sco.Discriminator.GetAllInclusions().All(i => dis.GetAllInclusions().Contains(i)))
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
                            }
                            else
                            {
                                permissions = permissions.Except(new[] { per });
                            }
                        }
                    }
                    Printer.Ident("Permissions to exclude:", () => res.Print(PrintMode.Table));
                    permissions = permissions.Except(res, new PermissionEqualityComparer());
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
        internal static void CheckFunctionAssigned(this IRol me, IFunction function, IDiscriminator[] discriminators, Action<string, bool> console)
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

        #region Fluent
        #region IRol
        public static IRolCan Can(this IRol me, params IFunction[] functions) { return new _RolCan(me, functions); }
        public static IRolEnsureCan EnsureCan(this IRol me, params IFunction[] functions) { return new _RolCan(me, functions); }
        #endregion
        #region IRolCan
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
        #endregion
        #region IRolEnsureCan
        public static void This<T>(this IRolEnsureCan me, T value) { me.These<T>(new[] { value }); }
        public static void These<T>(this IRolEnsureCan me, IEnumerable<T> values) { me.These(values.ToArray()); }
        public static void These<T>(this IRolEnsureCan me, params T[] values) {
            var r = me as _RolCan;
            r.Rol.FilterExpression<T>(r.Functions);
            if (!values.AuthorizedTo(r.Functions).Any())
                throw new UnauthorizedAccessException($"The rol '{r.Rol.Name}' cannot '{r.Functions.Aggregate("", (a, c) => a + c.Name + "·", a => a.Trim('·'))}' for the given instances '{values}'");
        }
        #endregion
        #endregion
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
    public interface IRolEnsureCan { }
    class _RolCan : IRolCan, IRolEnsureCan
    {
        public _RolCan(IRol rol, IFunction[] functions) { Rol = rol; Functions = functions; }
        public IRol Rol { get; set; }
        public IFunction[] Functions { get; set; }
    }
}
