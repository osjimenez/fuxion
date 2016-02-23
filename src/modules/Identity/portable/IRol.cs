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
        public static IEnumerable<IScope> GetScopes(this IRol me, IFunction function, IDiscriminator[] discriminators)
        {
            IEnumerable<IScope> res = null;
            Printer.Ident($"GetScopes ...", () =>
            {
                var pers = me.GetPermissions(function, discriminators);
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
        internal static IEnumerable<IPermission> GetPermissions(this IRol me, IFunction function, IDiscriminator[] discriminators)
        {
            var res = new List<IPermission>();
            Printer.Ident($"{nameof(GetPermissions)} ...", () =>
            {
                Printer.Print($"Filtering permissions:");
                //                "   Rols => " + rol.AllMembership().Aggregate("", (s, a) => s + a.Id + " , ", s => s.Trim(',', ' ')) + "\r\n" +
                //"   Rols => " + membership.Aggregate("", (s, a) => s + a + " , ", s => s.Trim(',', ' ')) + "\r\n" +
                Printer.Print($"Functions => " + function.GetAllExclusions().Aggregate("", (s, a) => s + a.Id + " , ", s => s.Trim(',', ' ')));
                Printer.Print($"Discriminators:");
                discriminators.Print(PrintMode.Table);
                //var permissions = me.Permissions.ToList().AsQueryable();
                var permissions = me.AllPermissions();
                Printer.Print("Initial permissions:");
                permissions.Print(PrintMode.Table);
                //PrintPermissions("      Initial permissions", permissions, 9, con);
                #region Filter by rol
                //permissions = permissions.Where(p =>
                //    // Exclude permissions for roles that i don't take
                //    membership.Contains(p.Rol)
                //    );
                //PrintPermissions("      After rol filters", permissions, 9, con);
                #endregion
                #region Filter by function
                permissions = permissions.Where(p =>
                    // Exclude permissions granted with functions excluded by me
                    (p.Value && function.GetAllExclusions().Contains(p.Function, new FunctionEqualityComparer()))
                        ||
                        // Or exclude permissions denied with functions included by me
                        (!p.Value && function.GetAllInclusions().Contains(p.Function, new FunctionEqualityComparer()))
                    );
                Printer.Print($"After function filter:");
                permissions.Print(PrintMode.Table);
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
                Printer.Print($"After discriminator Id filter:");
                permissions.Print(PrintMode.Table);
                #endregion
                #region Filter by Path
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
                            switch (sco.Propagation)
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
                        else if (!sco.Discriminator.GetAllInclusions().Any() ||
                                dis.GetAllInclusions().All(i => sco.Discriminator.GetAllInclusions().Contains(i)) ||
                                !dis.GetAllInclusions().SequenceEqual(sco.Discriminator.GetAllInclusions()))
                        //else if ((string.IsNullOrWhiteSpace(sco.DiscriminatorPath) ||
                        //            dis.Path.StartsWith(sco.DiscriminatorPath)) &&
                        //            dis.Path != sco.DiscriminatorPath)
                        {
                            if (sco.Propagation == ScopePropagation.ToInclusions) res.Add(per);
                            // My id is child of permission scope
                            //if (sco.PropagateToChilds) res.Add(per);
                        }
                        else if (sco.Discriminator.GetAllInclusions().All(i => dis.GetAllInclusions().Contains(i)))
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
                Printer.Print($"After discriminator Path filter:");
                permissions.Print(PrintMode.Table);
                #endregion
            });
            return res;
        }


        private static Expression<Func<TEntity, bool>> BuildForeignKeysContainsPredicate<TEntity, T>(List<T> foreignKeys, PropertyInfo property)
        {
            // TODO - Oscar - Este código representa la consulta que habría que hacer en caso de guardar la ruta completa en los recursos 
            // y solo cargar en el SecuritySchema los id's directamente implicados. Esto es un cambio importante que evitaría tener que cargar
            // todos los scopes al iniciar sesión, por ejemplo, el usuario root, como tienen permiso para los discriminaodres raiz tiene que cargar
            // todos los discriminaodres hijos (que son todos) al iniciar sesión.

            // var t1 =
            // Context.City.Include(c => c.AuthorizationDomain)
            //     .Where(c => new[] { "ruta1", "ruta2" }.Any(r => c.AuthorizationDomain.Path.StartsWith(r)));
            // Equivale a:
            //     .Where(c => foreignKeys.Any(r => c.<property>.Path.StartsWith(r)));
            var entityParameter = Expression.Parameter(typeof(TEntity));
            var foreignKeysParameter = Expression.Constant(foreignKeys, typeof(List<T>));
            var memberExpression = Expression.Property(entityParameter, property);
            var convertExpression = Expression.Convert(memberExpression, typeof(T));
            var containsExpression = Expression.Call(foreignKeysParameter, nameof(Enumerable.Contains), new Type[] { }, convertExpression);
            var result = Expression.Lambda<Func<TEntity, bool>>(containsExpression, entityParameter);
            if (
                // TODO - Oscar - Probar que la comprobación de INullable no afecta al resultado. INullable esta en System.Data.SqlTypes.INullable
                //typeof(INullable).IsAssignableFrom(property.PropertyType) ||
                (property.PropertyType.GetTypeInfo().IsGenericType &&
                 property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                var arg = Expression.Parameter(typeof(TEntity));
                var prop = Expression.Property(arg, property);

                //var ooo = Expression.Lambda<Func<TEntity, bool>>(
                //    Expression.NotEqual(prop, Expression.Constant(null, prop.Type)),
                //    arg);
                //var invokedExpr = Expression.Invoke(result, ooo.Parameters.Cast<Expression>());
                //var oooo = Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(ooo.Body, invokedExpr), ooo.Parameters);
                //result = oooo;

                result = Expression.Lambda<Func<TEntity, bool>>(
                    Expression.NotEqual(prop, Expression.Constant(null, prop.Type)),
                    arg)
                    .And(result);
            }
            return result;
        }
        public static Expression<Func<TEntity, bool>> FilterPredicate<TEntity>(this IRol me, IFunction function)
        {
            Expression<Func<TEntity, bool>> exp = null;
            Printer.Ident($"{nameof(FilterPredicate)} ...", () =>
            {
                Printer.Print("Type: " + typeof(TEntity).Name);
                var scopes = me.GetScopes(function, new[] { TypeDiscriminator.Create(typeof(TEntity)) });
                Printer.Print("Scopes:");
                scopes.Print(PrintMode.Table);

                var props = typeof(TEntity).GetTypeInfo().DeclaredProperties
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
                Printer.Ident("Starting process ...", () =>
                {
                    Printer.Print("(");
                    foreach (var sco in scopes)
                    {
                        //Printer.Print("Scope: " + sco);


                        if (exp != null) Printer.Print(")AND(");

                        //Expression<Func<TEntity, bool>> disExp = null;
                        //var alldis = sco.AllDiscriminators().ToList();
                        //var alldis2 = sco.Discriminator.GetAllInclusions();
                        //foreach (var dis in new[] { sco.Discriminator })
                        //{
                        //Printer.Print("Discriminator: " + dis);

                        //if (disExp == null) Printer.Print("(");
                        //if (disExp != null) Printer.Print(")AND(");
                        Expression<Func<TEntity, bool>> proExp = null;
                        Printer.Ident(() =>
                        {
                            foreach (var pro in props.Where(p => p.DiscriminatorTypeId.Equals(sco.Discriminator.TypeId)))
                            {
                                //if (proExp == null)
                                    Printer.Print("    " +
                                                    sco.Discriminator.GetAllInclusions().Select(d => d.Id).Aggregate("",
                                                        (s, a) => s + pro.PropertyInfo.Name + " == " + a + " || ",
                                                        s => s.Trim('|', ' ')));
                                

                                var curExp = (Expression<Func<TEntity, bool>>)
                                    typeof(RolExtensions).GetTypeInfo()
                                        .DeclaredMethods.Single(m => m.Name == nameof(BuildForeignKeysContainsPredicate))
                                        .MakeGenericMethod(pro.PropertyInfo.DeclaringType, pro.PropertyInfo.PropertyType)
                                        .Invoke(null, new object[] {
                                            typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                                .Where(m => m.Name == nameof(Enumerable.ToList))
                                                .Single(m => m.GetParameters().Length == 1)
                                                .MakeGenericMethod(pro.PropertyInfo.PropertyType)
                                                .Invoke(null, new object[] {
                                                    typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                                    .Where(m => m.Name == nameof(Enumerable.Cast))
                                                    .Single(m => m.GetParameters().Length == 1)
                                                    .MakeGenericMethod(pro.PropertyInfo.PropertyType)
                                                    .Invoke(null,new object[] {
                                                        sco.Discriminator.GetAllInclusions().Select(d => d.Id)
                                                    })
                                                }),
                                            pro.PropertyInfo
                                            }
                                        );
                                
                                proExp = (proExp == null ? curExp : proExp.Or(curExp));
                                if (proExp != null) Printer.Print(") OR (");
                            }
                        });
                        //if (disExp != null) Printer.Print(")");
                        //disExp = (disExp == null ? proExp : disExp.And(proExp));
                        //}
                        if (exp != null) Printer.Print(")");
                        //exp = (exp == null ? disExp : exp.And(disExp));
                        exp = (exp == null ? proExp : exp.And(proExp));
                    }
                });
                if (exp == null && !scopes.Any()) exp = Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(false), Expression.Parameter(typeof(TEntity)));
                Printer.Print("Expression:" + (exp == null ? "null" : exp.ToString()));
            });
            return exp;
        }



        public static IEnumerable<IPermission> AllPermissions(this IRol me)
        {
            var res = new List<IPermission>();
            res.AddRange(me.Permissions);
            foreach (var gro in me.Groups) res.AddRange(gro.AllPermissions());
            return res;
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
