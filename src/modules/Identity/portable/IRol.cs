using System;
using System.Collections.Generic;
using Fuxion.Identity.Helpers;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Linq.Expressions;
using Fuxion.Factories;
using System.Collections;
using System.Runtime.CompilerServices;

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
        public static IEnumerable<Tuple<bool,IScope>> GetScopes(this IRol me, IFunction[] functions, IDiscriminator[] discriminators)
        {
            return Printer.Ident($"{typeof(RolExtensions).GetTypeInfo().DeclaredMethods.FirstOrDefault(m => m.Name == nameof(GetScopes)).GetSignature()}:", () =>
            {
                Printer.Ident("Input parameters:", () => {
                    Printer.WriteLine("Rol:");
                    new[] { me }.Print(PrintMode.Table);
                    Printer.WriteLine("Functions:");
                    functions.Print(PrintMode.Table);
                    Printer.WriteLine("Discriminators");
                    discriminators.Print(PrintMode.Table);
                });
                var pers = me.GetPermissions(functions, discriminators);
                var granted = pers.Where(p => p.Value);
                var denied = pers.Where(p => !p.Value);
                Printer.WriteLine("Granted permissions:");
                granted.Print(PrintMode.Table);
                Printer.WriteLine($"Denied permissions:");
                denied.Print(PrintMode.Table);
                var res = pers.SelectMany(p => p.Scopes.Select(s=>new Tuple<bool, IScope>(p.Value,s)));
                return res;
            });
        }
        internal static IEnumerable<IPermission> GetPermissions(this IRol me, IFunction[] functions, IDiscriminator[] discriminators)
        {
            
            return Printer.Ident($"{typeof(RolExtensions).GetTypeInfo().DeclaredMethods.FirstOrDefault(m => m.Name == nameof(GetPermissions)).GetSignature()}:", () =>
            {
                Printer.Ident("Input parameters:", () => {
                    Printer.WriteLine("Rol:");
                    new[] { me }.Print(PrintMode.Table);
                    Printer.WriteLine("Functions:");
                    functions.Print(PrintMode.Table);
                    Printer.WriteLine("Discriminators");
                    discriminators.Print(PrintMode.Table);
                });
                var permissions = me.AllPermissions();
                var exclusions = functions.SelectMany(f => f.GetAllExclusions().Union(new[] { f })).Distinct();
                var inclusions = functions.SelectMany(f => f.GetAllExclusions().Union(new[] { f })).Distinct();
                Printer.WriteLine("Functions: " + exclusions.Aggregate("", (s, a) => s + a.Id + " , ", s => s.Trim(',', ' ')));
                Printer.Ident("Discriminators:", () => discriminators.Print(PrintMode.Table));
                Printer.Ident("Initial permissions:", () => permissions.Print(PrintMode.Table));
                Printer.WriteLine("");
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
                Printer.Ident("Returned permissions:", () => permissions.Print(PrintMode.Table));
                return permissions;
            });
        }
        internal static Expression<Func<TEntity, bool>> FilterExpression<TEntity>(this IRol me, IFunction[] functions)
        {
            return Printer.Ident($"{typeof(RolExtensions).GetTypeInfo().DeclaredMethods.FirstOrDefault(m => m.Name == nameof(FilterExpression)).GetSignature()}:", () =>
            {
                Printer.Enabled = false;
                Printer.Ident("Input parameters:", () =>
                {
                    Printer.WriteLine("Rol:");
                    new[] { me }.Print(PrintMode.Table);
                    Printer.WriteLine("Functions:");
                    functions.Print(PrintMode.Table);
                    Printer.WriteLine("Type: " + typeof(TEntity).Name);
                });
                var scopes = me.GetScopes(functions, new[] { Factory.Get<TypeDiscriminatorFactory>().FromType(typeof(TEntity)) }).ToList();
                var props = typeof(TEntity).GetRuntimeProperties()
                    .Where(p => p.GetCustomAttribute<DiscriminatedByAttribute>(true, false, false) != null)
                    .Select(p => new
                    {
                        Info = p,
                        Type = p.PropertyType,
                        DiscriminatorType = p.GetCustomAttribute<DiscriminatedByAttribute>(true).Type,
                        DiscriminatorTypeId =
                            p.GetCustomAttribute<DiscriminatedByAttribute>(true).Type.GetTypeInfo()
                                .GetCustomAttribute<DiscriminatorAttribute>(true).TypeId,
                    });
                Printer.WriteLine("Properties => " + props.Aggregate("", (str, actual) => $"{str} {actual.Info.Name} ({actual.Info.PropertyType.GetSignature(false)}),", str => str.Trim(',', ' ')));
                Expression<Func<TEntity, bool>> res = null;

                Printer.Enabled = true;

                foreach(var sco in scopes)
                {
                    Expression<Func<TEntity, bool>> proExp = null;
                    var propsOfType = props.Where(p => p.DiscriminatorTypeId.Equals(sco.Item2.Discriminator.TypeId)).ToList();
                    foreach(var pro in propsOfType)
                    {
                        var castMethod = typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                            .Where(m => m.Name == nameof(Enumerable.Cast))
                                            .Single(m => m.GetParameters().Length == 1)
                                            .MakeGenericMethod(pro.Type);
                        var ofTypeMethod = typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                            .Where(m => m.Name == nameof(Enumerable.OfType))
                                            .Single(m => m.GetParameters().Length == 1)
                                            .MakeGenericMethod(pro.DiscriminatorType);
                        var toListMethod = typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                            .Where(m => m.Name == nameof(Enumerable.ToList))
                                            .Single(m => m.GetParameters().Length == 1)
                                            .MakeGenericMethod(pro.Type);
                        var containsMethod = typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                            .Where(m => m.Name == nameof(Enumerable.Contains))
                                            .Single(m => m.GetParameters().Length == 2)
                                            .MakeGenericMethod(pro.Type);
                        IEnumerable<IDiscriminator> foreignDiscriminators = Enumerable.Empty<IDiscriminator>();
                        if (sco.Item2.Propagation.HasFlag(ScopePropagation.ToMe))
                            foreignDiscriminators = foreignDiscriminators.Union(new[] { sco.Item2.Discriminator });
                        if (sco.Item2.Propagation.HasFlag(ScopePropagation.ToInclusions))
                            foreignDiscriminators = foreignDiscriminators.Union(sco.Item2.Discriminator.GetAllInclusions().Select(d => d));
                        if (sco.Item2.Propagation.HasFlag(ScopePropagation.ToExclusions))
                            foreignDiscriminators = foreignDiscriminators.Union(sco.Item2.Discriminator.GetAllExclusions().Select(d => d));
                        var foreignDiscriminatorssOfType = (IEnumerable<IDiscriminator>)ofTypeMethod.Invoke(null, new object[] { foreignDiscriminators });
                        // Si no hay claves externas del tipo de esta propiedad, continuo con la siguiente propiedad
                        if (!foreignDiscriminatorssOfType.Any()) continue;
                        var foreignKeys = foreignDiscriminatorssOfType.Select(d => d.Id);
                        var foreignKeysCasted = castMethod.Invoke(null, new object[] { foreignKeys });
                        var foreignKeysListed = toListMethod.Invoke(null, new object[] { foreignKeysCasted });
                        

                        var entityParameter = Expression.Parameter(typeof(TEntity));
                        var memberExpression = Expression.Property(entityParameter, pro.Info);
                        var containsExpression = Expression.Call(containsMethod,
                            Expression.Constant(foreignKeysListed, foreignKeysListed.GetType()),
                            memberExpression);
                        var curExp = Expression.Lambda<Func<TEntity, bool>>((sco.Item1 ? (Expression)containsExpression : Expression.Not(containsExpression)), entityParameter);
                        if (
                            (pro.Type.GetTypeInfo().IsGenericType &&
                             pro.Type.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {
                            var arg = Expression.Parameter(typeof(TEntity));
                            var prop = Expression.Property(arg, pro.Info);
                            curExp = Expression.Lambda<Func<TEntity, bool>>(
                                Expression.NotEqual(prop, Expression.Constant(null, prop.Type)),
                                arg)
                                .And(curExp);
                        }
                        proExp = (proExp == null ? curExp : (sco.Item1 ? proExp.Or(curExp) : proExp.And(curExp)));
                    }
                    if (proExp != null)
                        res = (res == null ? proExp : res.And(proExp));
                }
                #region OLD CODE
                /*
                Printer.Ident("Filter:", () =>
                {
                    Printer.WriteLine("(");
                    foreach (var sco in scopes.Where(sc => sc.Item1))
                    {
                        if (res != null) Printer.WriteLine("AND(");
                        Expression<Func<TEntity, bool>> proExp = null;
                        Printer.Ident(() =>
                        {
                            var propsOfType = props.Where(p => p.DiscriminatorTypeId.Equals(sco.Item2.Discriminator.TypeId)).ToList();
                            for (int p = 0; p < propsOfType.Count; p++)
                            {
                                var pro = propsOfType[p].PropertyInfo;
                                //var att = pro.GetCustomAttribute<DiscriminatedByAttribute>();

                                var castMethod = typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                                    .Where(m => m.Name == nameof(Enumerable.Cast))
                                                    .Single(m => m.GetParameters().Length == 1)
                                                    .MakeGenericMethod(pro.PropertyType);
                                var ofTypeMethod = typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                                    .Where(m => m.Name == nameof(Enumerable.OfType))
                                                    .Single(m => m.GetParameters().Length == 1)
                                                    .MakeGenericMethod(propsOfType[p].DiscriminatorType);
                                var toListMethod = typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                                    .Where(m => m.Name == nameof(Enumerable.ToList))
                                                    .Single(m => m.GetParameters().Length == 1)
                                                    .MakeGenericMethod(pro.PropertyType);
                                var containsMethod = typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                                    .Where(m => m.Name == nameof(Enumerable.Contains))
                                                    .Single(m => m.GetParameters().Length == 2)
                                                    .MakeGenericMethod(pro.PropertyType);
                                IEnumerable<IDiscriminator> foreignDiscriminators = Enumerable.Empty<IDiscriminator>();
                                if (sco.Item2.Propagation.HasFlag(ScopePropagation.ToMe))
                                    foreignDiscriminators = foreignDiscriminators.Union(new[] { sco.Item2.Discriminator });
                                if (sco.Item2.Propagation.HasFlag(ScopePropagation.ToInclusions))
                                    foreignDiscriminators = foreignDiscriminators.Union(sco.Item2.Discriminator.GetAllInclusions().Select(d => d));
                                if (sco.Item2.Propagation.HasFlag(ScopePropagation.ToExclusions))
                                    foreignDiscriminators = foreignDiscriminators.Union(sco.Item2.Discriminator.GetAllExclusions().Select(d => d));
                                var foreignDiscriminatorssOfType = (IEnumerable<IDiscriminator>)ofTypeMethod.Invoke(null, new object[] { foreignDiscriminators });
                                var foreignKeys = foreignDiscriminatorssOfType.Select(d => d.Id);
                                Printer.WriteLine("    " +
                                    foreignKeys.Select(d => d).Aggregate("",
                                        (s2, a) => s2 + pro.Name + " == " + a + " || ",
                                        s2 => s2.Trim('|', ' ')));
                                //var foreignKeys = sco.Discriminator.GetAllInclusions().Select(d => d.Id);
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
                                        Printer.WriteLine(")");
                                    else
                                        Printer.WriteLine(") OR (");
                                }
                            }
                        });
                        if (res != null) Printer.WriteLine(")");
                        if (proExp != null)
                            res = (res == null ? proExp : res.And(proExp));
                    }
                    foreach (var sco in scopes.Where(sc => !sc.Item1))
                    {
                        if (res != null) Printer.WriteLine("AND(");
                        Expression<Func<TEntity, bool>> proExp = null;
                        Printer.Ident(() =>
                        {
                            var propsOfType = props.Where(p => p.DiscriminatorTypeId.Equals(sco.Item2.Discriminator.TypeId)).ToList();
                            for (int p = 0; p < propsOfType.Count; p++)
                            {
                                var pro = propsOfType[p].PropertyInfo;
                                //var att = pro.GetCustomAttribute<DiscriminatedByAttribute>();

                                var castMethod = typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                                    .Where(m => m.Name == nameof(Enumerable.Cast))
                                                    .Single(m => m.GetParameters().Length == 1)
                                                    .MakeGenericMethod(pro.PropertyType);
                                var ofTypeMethod = typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                                    .Where(m => m.Name == nameof(Enumerable.OfType))
                                                    .Single(m => m.GetParameters().Length == 1)
                                                    .MakeGenericMethod(propsOfType[p].DiscriminatorType);
                                var toListMethod = typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                                    .Where(m => m.Name == nameof(Enumerable.ToList))
                                                    .Single(m => m.GetParameters().Length == 1)
                                                    .MakeGenericMethod(pro.PropertyType);
                                var containsMethod = typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                                    .Where(m => m.Name == nameof(Enumerable.Contains))
                                                    .Single(m => m.GetParameters().Length == 2)
                                                    .MakeGenericMethod(pro.PropertyType);
                                IEnumerable<IDiscriminator> foreignDiscriminators = Enumerable.Empty<IDiscriminator>();
                                if (sco.Item2.Propagation.HasFlag(ScopePropagation.ToMe))
                                    foreignDiscriminators = foreignDiscriminators.Union(new[] { sco.Item2.Discriminator });
                                if (sco.Item2.Propagation.HasFlag(ScopePropagation.ToInclusions))
                                    foreignDiscriminators = foreignDiscriminators.Union(sco.Item2.Discriminator.GetAllInclusions().Select(d => d));
                                if (sco.Item2.Propagation.HasFlag(ScopePropagation.ToExclusions))
                                    foreignDiscriminators = foreignDiscriminators.Union(sco.Item2.Discriminator.GetAllExclusions().Select(d => d));
                                var foreignDiscriminatorssOfType = (IEnumerable<IDiscriminator>)ofTypeMethod.Invoke(null, new object[] { foreignDiscriminators });
                                var foreignKeys = foreignDiscriminatorssOfType.Select(d => d.Id);
                                Printer.WriteLine("    " +
                                    foreignKeys.Select(d => d).Aggregate("",
                                        (s2, a) => s2 + pro.Name + " == " + a + " || ",
                                        s2 => s2.Trim('|', ' ')));
                                //var foreignKeys = sco.Discriminator.GetAllInclusions().Select(d => d.Id);
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
                                        Printer.WriteLine(")");
                                    else
                                        Printer.WriteLine(") OR (");
                                }
                            }
                        });
                        if (res != null) Printer.WriteLine(")");
                        if (proExp != null)
                            res = (res == null ? proExp : res.And(proExp));
                    }
                });
                */
                #endregion
                if (res == null && !scopes.Any())
                {
                    if (me.GetPermissions(functions, new[] { Factory.Get<TypeDiscriminatorFactory>().FromType(typeof(TEntity)) }).Any())
                    {
                        res = Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(true), Expression.Parameter(typeof(TEntity)));
                    }
                    else
                    {
                        res = Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(false), Expression.Parameter(typeof(TEntity)));
                    }
                }
                Printer.WriteLine("Expression:");
                PrintExpression(res.Body);
                return res;
            });
        }
        private static void PrintExpression(Expression exp)
        {
            //string res = "";
            if (exp is BinaryExpression)
            {
                PrintBinaryExpression(exp as BinaryExpression);
            }
            else if (exp is MethodCallExpression)
            {
                PrintMethodCallExpression(exp as MethodCallExpression);
            }
            else if (exp is ConstantExpression)
            {
                PrintConstantExpression(exp as ConstantExpression);
            }
            else if (exp is MemberExpression)
            {
                PrintMemberExpression(exp as MemberExpression);
            }
            else if (exp is UnaryExpression)
            {
                PrintUnaryExpression(exp as UnaryExpression);
            }
            else
            {
                Printer.WriteLine($"'{exp.GetType().Name}'");
            }
            //return res;
        }
        private static void PrintBinaryExpression(BinaryExpression exp)
        {
            Printer.Ident("(", () => {
                PrintExpression(exp.Left);
                if (Printer.IsLineWritePending) Printer.WriteLine("");
                Printer.WriteLine(exp.NodeType.ToString().ToUpper());
                PrintExpression(exp.Right);
                if (Printer.IsLineWritePending) Printer.WriteLine("");
            });
            if (Printer.IsLineWritePending) Printer.WriteLine("");
            Printer.WriteLine(")");
        }
        private static void PrintMethodCallExpression(MethodCallExpression exp)
        {
            var isExtenssionMethod = exp.Method.GetCustomAttribute<ExtensionAttribute>() != null;
            if (isExtenssionMethod)
            {
                if (exp.Method.Name == "Contains")
                {
                    foreach (var e in exp.Arguments)
                    {
                        PrintExpression(e);
                    }
                }
                else
                {
                    PrintExpression(exp.Arguments[0]);
                    Printer.Write($".{exp.Method.Name}(");
                    foreach (var e in exp.Arguments.Skip(1))
                    {
                        PrintExpression(e);
                    }
                }
            }
            else
            {
                Printer.Write($"{exp.Method.Name}(");
                foreach (var e in exp.Arguments)
                {
                    PrintExpression(e);
                }
            }
            Printer.Write(")");
        }
        private static void PrintConstantExpression(ConstantExpression exp)
        {
            string res = "";
            if (exp.Value.GetType().IsSubclassOfRawGeneric(typeof(List<>)))
            {
                var list = exp.Value as IEnumerable<object>;
                res += list.Aggregate("[", (a, c) => a + c + ", ", a => a.Trim(' ', ',') +"]");
            }
            else
            {
                res += exp.Value;
            }
            Printer.Write(res);
        }
        private static void PrintMemberExpression(MemberExpression exp)
        {
            string res = "";
            res += exp.Member.Name;
            Printer.Write(res);
        }
        private static void PrintUnaryExpression(UnaryExpression exp)
        {
            switch (exp.NodeType)
            {
                case ExpressionType.Not:
                    Printer.Write("!");
                    break;
                default:
                    Printer.Write("<undefined>");
                    break;
            }
            PrintExpression(exp.Operand);
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
        private static IPermission[] SearchForDeniedPermissions(this IRol me, IFunction function, IDiscriminator[] discriminators)//, Action<string, bool> console)
        {
            //Action<string, bool> con = (m, i) => { console?.Invoke(m, i); };
            return Printer.Ident($"SearchForDeniedPermissions:", () =>
            {
                Printer.Ident("Input parameters", () =>
                {
                    Printer.WriteLine($"Rol: {me.Name}");
                    Printer.WriteLine($"Function: {function.Name}");
                    Printer.Foreach($"Discriminadores:", discriminators, dis => Printer.WriteLine($"{dis.TypeName} - {dis.Name}"));
                });
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
                Printer.Foreach($"Rol permissions ({permissions.Count()}):", permissions, p =>
                {
                    Printer.WriteLine("Permission: " + p);
                });
                IPermission[] res = null;
                // First, must take only permissions that match with the function and discriminators
                if (permissions == null || !permissions.Any())
                {
                    Printer.WriteLine($"Resultado: NO VALIDO - El rol no tiene permisos definidos");
                    res = new IPermission[] { };
                    return res;
                }
                Printer.WriteLine("Comprobando los permisos que encajan con los discriminadores de entrada");
                var matchs = permissions.Where(p => p.Match(function, discriminators)).ToList();
                Printer.WriteLine($"Encajan '{matchs.Count}' permisos");
                // Now, let's go to check that does not match any denegation permission
                res = matchs.Where(p => !p.Value).ToArray();
                Printer.WriteLine($"'{res.Length}' permisos son de denegación");
                if (res.Any())
                {
                    Printer.WriteLine($"Resultado: NO VALIDO - Denegado por un permiso");
                    return res;
                }
                // Now, let's go to check that match at least one grant permission
                if (!matchs.Any(p => p.Value))
                {
                    Printer.WriteLine($"Resultado: NO VALIDO - Denegado por no encontrar un permiso de concesión");
                    res = new IPermission[] { };
                    return res;
                }
                Printer.WriteLine($"Resultado: VALIDO");
                return null;
            });
        }
        internal static bool IsFunctionAssigned(this IRol me, IFunction function, IDiscriminator[] discriminators)
        {
            return SearchForDeniedPermissions(me, function, discriminators) == null;
        }
        public static void Print(this IEnumerable<IRol> me, PrintMode mode)
        {
            switch (mode)
            {
                case PrintMode.OneLine:
                    break;
                case PrintMode.PropertyList:
                    break;
                case PrintMode.Table:
                    var nameLength = me.Select(s => s.Name.ToString().Length).Union(new[] { "NAME".Length }).Max();
                    //var typeName = me.Select(s => s.TypeName.Length).Union(new[] { "TYPE_NAME".Length }).Max();
                    //var id = me.Select(s => s.Id.ToString().Length).Union(new[] { "ID".Length }).Max();
                    //var name = me.Select(s => s.Name.Length).Union(new[] { "ID".Length }).Max();
                    Printer.WriteLine("┌" + ("".PadRight(nameLength, '─')) + "┐");
                    Printer.WriteLine("│" + "NAME".PadRight(nameLength, ' ') + "│");
                    Printer.WriteLine("├" + ("".PadRight(nameLength, '─')) + "┤");
                    foreach (var rol in me) Printer.WriteLine("│" + rol.Name.ToString().PadRight(nameLength, ' ') + "│");
                    Printer.WriteLine("└" + ("".PadRight(nameLength, '─')) + "┘");
                    break;
            }
        }

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
                    new[] { Factory.Get<TypeDiscriminatorFactory>().FromType(t) }))
                : types.Any(t => me.Rol.IsFunctionAssigned(
                    me.Functions.First(),
                    new[] { Factory.Get<TypeDiscriminatorFactory>().FromType(t) }));
            if (me.ThrowExceptionIfCannot && !res)
                throw new UnauthorizedAccessException($"The rol '{me.Rol.Name}' cannot '{me.Functions.Aggregate("", (a, c) => a + c.Name + "·", a => a.Trim('·'))}' for the given types '{types.Aggregate("", (a, c) => $"{a}, {c.Name}", a => a.Trim(',', ' ')) }'");
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
            var res = forAll ? values.AuthorizedTo(me.Rol, me.Functions).Count() == values.Count() : values.AuthorizedTo(me.Rol, me.Functions).Any();
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
