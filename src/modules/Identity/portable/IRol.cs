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
        internal static Expression<Func<TEntity, bool>> FilterExpression<TEntity>(this IRol me, IFunction[] functions)
        {
            return Printer.Indent($"{typeof(RolExtensions).GetTypeInfo().DeclaredMethods.FirstOrDefault(m => m.Name == nameof(FilterExpression)).GetSignature()}:", () =>
            {
                Printer.Indent("Input parameters:", () =>
                {
                    Printer.WriteLine("Rol:");
                    new[] { me }.Print(PrintMode.Table);
                    Printer.WriteLine("Functions:");
                    functions.Print(PrintMode.Table);
                    Printer.WriteLine("Type: " + typeof(TEntity).Name);
                });
                #region Methods
                MethodInfo GetCastMethod(Type type) =>
                    typeof(Enumerable).GetTypeInfo().DeclaredMethods
                        .Where(m => m.Name == nameof(Enumerable.Cast))
                        .Single(m => m.GetParameters().Length == 1)
                        .MakeGenericMethod(type);
                MethodInfo GetOfTypeMethod(Type type) =>
                    typeof(Enumerable).GetTypeInfo().DeclaredMethods
                        .Where(m => m.Name == nameof(Enumerable.OfType))
                        .Single(m => m.GetParameters().Length == 1)
                        .MakeGenericMethod(type);
                MethodInfo GetToListMethod(Type type) =>
                    typeof(Enumerable).GetTypeInfo().DeclaredMethods
                        .Where(m => m.Name == nameof(Enumerable.ToList))
                        .Single(m => m.GetParameters().Length == 1)
                        .MakeGenericMethod(type);
                MethodInfo GetContainsMethod(Type type) =>
                    typeof(Enumerable).GetTypeInfo().DeclaredMethods
                        .Where(m => m.Name == nameof(Enumerable.Contains))
                        .Single(m => m.GetParameters().Length == 2)
                        .MakeGenericMethod(type);
                Expression<Func<TEntity, bool>> GetContainsExpression(bool value, IScope sco, Type disType, PropertyInfo proInfo){
                    IEnumerable<IDiscriminator> foreignDiscriminators = Enumerable.Empty<IDiscriminator>();
                    if (sco.Propagation.HasFlag(ScopePropagation.ToMe))
                        foreignDiscriminators = foreignDiscriminators.Union(new[] { sco.Discriminator });
                    if (sco.Propagation.HasFlag(ScopePropagation.ToInclusions))
                        foreignDiscriminators = foreignDiscriminators.Union(sco.Discriminator.GetAllInclusions().Select(d => d));
                    if (sco.Propagation.HasFlag(ScopePropagation.ToExclusions))
                        foreignDiscriminators = foreignDiscriminators.Union(sco.Discriminator.GetAllExclusions().Select(d => d));
                    var foreignDiscriminatorssOfType = (IEnumerable<IDiscriminator>)GetOfTypeMethod(disType).Invoke(null, new object[] { foreignDiscriminators });
                    // Si no hay claves externas del tipo de esta propiedad, continuo con la siguiente propiedad
                    if (!foreignDiscriminatorssOfType.Any()) return null;
                    var foreignKeys = foreignDiscriminatorssOfType.Select(d => d.Id);
                    var foreignKeysCasted = GetCastMethod(proInfo.PropertyType).Invoke(null, new object[] { foreignKeys });
                    var foreignKeysListed = GetToListMethod(proInfo.PropertyType).Invoke(null, new object[] { foreignKeysCasted });


                    var entityParameter = Expression.Parameter(typeof(TEntity));
                    var memberExpression = Expression.Property(entityParameter, proInfo);
                    var containsExpression = Expression.Call(GetContainsMethod(proInfo.PropertyType),
                        Expression.Constant(foreignKeysListed, foreignKeysListed.GetType()),
                        memberExpression);
                    var curExp = Expression.Lambda<Func<TEntity, bool>>((value ? (Expression)containsExpression : Expression.Not(containsExpression)), entityParameter);
                    return curExp;
                }
                #endregion 
                var props = typeof(TEntity).GetRuntimeProperties()
                   .Where(p => p.GetCustomAttribute<DiscriminatedByAttribute>(true, false, false) != null)
                   .Select(p => new
                   {
                       PropertyInfo = p,
                       PropertyType = p.PropertyType,
                       DiscriminatorType = p.GetCustomAttribute<DiscriminatedByAttribute>(true).Type,
                       DiscriminatorTypeId =
                           p.GetCustomAttribute<DiscriminatedByAttribute>(true).Type.GetTypeInfo()
                               .GetCustomAttribute<DiscriminatorAttribute>(true).TypeId,
                   });
                Printer.Foreach("Properties:", props, p => Printer.WriteLine($"{p.PropertyType.Name} {p.PropertyInfo.Name} - {p.DiscriminatorTypeId} {p.DiscriminatorType.Name}"));
                Expression<Func<TEntity, bool>> res = null;
                var pers = functions.SelectMany(fun => me.SearchPermissions(fun, Factory.Get<TypeDiscriminatorFactory>().FromType<TEntity>())).Distinct().ToList();
                Expression<Func<TEntity, bool>> denyPersExp = null;
                Printer.Foreach("Deny permissions:", pers.Where(p => !p.Value), per =>
                {
                    Expression<Func<TEntity, bool>> perExp = null;
                    Printer.Indent($"Permission: {per}", () =>
                    {
                        Printer.Foreach("Scopes:", per.Scopes, sco =>
                        {
                            Printer.Indent($"Scope: {sco}", () =>
                            {
                                // Recorro las propiedades que son del tipo de este discriminador
                                foreach (var pro in props.Where(p => p.DiscriminatorTypeId.Equals(sco.Discriminator.TypeId)).ToList())
                                {
                                    Printer.WriteLine($"Property: {pro.PropertyType.Name} {pro.PropertyInfo.Name}");
                                    var exp = GetContainsExpression(per.Value, sco, pro.DiscriminatorType, pro.PropertyInfo);
                                    if (exp == null) exp = Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(false), Expression.Parameter(typeof(TEntity)));
                                    if (perExp == null)
                                        perExp = exp;
                                    else
                                        perExp = perExp.And(exp);
                                }
                            });
                        });
                        //if (!per.Scopes.Any())
                        if (perExp == null)
                        {
                            perExp = Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(false), Expression.Parameter(typeof(TEntity)));
                        }
                    });
                    if (perExp != null)
                    {
                        if (denyPersExp != null)
                            denyPersExp = denyPersExp.And(perExp);
                        else
                            denyPersExp = perExp;
                    }
                });
                Expression<Func<TEntity, bool>> grantPersExp = null;
                Printer.Foreach("Grant permissions:", pers.Where(p => p.Value), per =>
                {
                    
                    Printer.Indent($"Permission: Value<{per.Value}> - Function<{per.Function.Name}> - Scopes<{per.Scopes.Count()}>", () =>
                    {
                        Expression<Func<TEntity, bool>> perExp = null;
                        Printer.Foreach("Scopes:", per.Scopes, sco =>
                        {
                            Printer.Indent($"Scope: {sco}", () =>
                            {
                                // Recorro las propiedades que son del tipo de este discriminador
                                foreach (var pro in props.Where(p => p.DiscriminatorTypeId.Equals(sco.Discriminator.TypeId)).ToList())
                                {
                                    Printer.WriteLine($"Property: {pro.PropertyType.Name} {pro.PropertyInfo.Name}");
                                    var exp = GetContainsExpression(per.Value, sco, pro.DiscriminatorType, pro.PropertyInfo);
                                    if (exp == null) exp = Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(false), Expression.Parameter(typeof(TEntity)));
                                    if (perExp == null)
                                        perExp = exp;
                                    else
                                        perExp = perExp.Or(exp);
                                }
                            });
                        });
                        //if (!per.Scopes.Any())
                        if (perExp == null)
                        {
                            perExp = Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(true), Expression.Parameter(typeof(TEntity)));
                        }
                        if (perExp != null)
                        {
                            if (grantPersExp != null)
                                grantPersExp = grantPersExp.Or(perExp);
                            else
                                grantPersExp = perExp;
                        }
                    });
                });
                if (denyPersExp != null && grantPersExp != null) res = denyPersExp.And(grantPersExp);
                if (denyPersExp != null && grantPersExp == null) res = denyPersExp;
                if (denyPersExp == null && grantPersExp != null) res = grantPersExp;
                res = res ?? Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(false), Expression.Parameter(typeof(TEntity)));

                Printer.WriteLine("Expression:");
                PrintExpression(res?.Body);
                if (Printer.IsLineWritePending) Printer.WriteLine("");

                return res;
            });
        }
        private static void PrintExpression(Expression exp)
        {
            if (exp == null) Printer.WriteLine("NULL");
            else if (exp is BinaryExpression)
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
        }
        private static void PrintBinaryExpression(BinaryExpression exp)
        {
            Printer.Indent("(", () =>
            {
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
                //if (exp.Method.Name == "Contains")
                //{
                //    foreach (var e in exp.Arguments)
                //    {
                //        PrintExpression(e);
                //    }
                //}
                //else
                //{
                PrintExpression(exp.Arguments[0]);
                Printer.Write($".{exp.Method.Name}(");
                foreach (var e in exp.Arguments.Skip(1))
                {
                    PrintExpression(e);
                }
                //}
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
            if (exp.Value == null)
                res += "null";
            else if (exp.Value.GetType().IsSubclassOfRawGeneric(typeof(List<>)))
            {
                string toAdd = "[";
                foreach (var obj in (IEnumerable)exp.Value)
                {
                    toAdd += obj + ", ";
                }
                toAdd = toAdd.Trim(' ', ',') + "]";
                res += toAdd;
                //var list = (IEnumerable<object>)exp.Value;
                //res += list.Aggregate("[", (a, c) => a + c + ", ", a => a.Trim(' ', ',') +"]");
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
        internal static IPermission[] SearchPermissions(this IRol me, IFunction function = null, params IDiscriminator[] discriminators)
        {
            discriminators = discriminators.RemoveNulls();
            return Printer.Indent($"{nameof(RolExtensions)}.{nameof(SearchPermissions)}:", () =>
            {
                Printer.Indent("Input parameters", () =>
                {
                    Printer.WriteLine($"Rol: {me.Name}");
                    Printer.WriteLine($"Function: {function?.Name ?? "<null>"}");
                    Printer.Foreach($"Discriminators:", discriminators, dis => Printer.WriteLine($"{dis.TypeName} - {dis.Name}"));
                });
                // Function validation
                if(!(function?.IsValid() ?? true)) throw new ArgumentException($"The '{nameof(function)}' pararameter with value '{function}' has an invalid state", nameof(function));
                // Discriminators validation
                //if (discriminators == null || !discriminators.Any()) throw new ArgumentException($"The '{nameof(discriminators)}' pararameter cannot be null or empty", nameof(discriminators));
                var invalidDiscriminator = discriminators?.FirstOrDefault(d => !d.IsValid());
                if (invalidDiscriminator != null) throw new InvalidStateException($"The '{invalidDiscriminator}' discriminator has an invalid state");

                // Load all permissions recursively
                IEnumerable<IPermission> GetPermissions(IRol rol)
                {
                    var r = new List<IPermission>();
                    if (rol.Permissions != null) r.AddRange(rol.Permissions);
                    if (rol.Groups != null) foreach (var gro in rol.Groups) r.AddRange(GetPermissions(gro));
                    return r;
                }
                var permissions = GetPermissions(me);
                Printer.Indent("Permissions:", () => permissions.Print(PrintMode.Table));
                //Printer.Foreach($"Rol permissions ({permissions.Count()}):", permissions, p =>
                //{
                //    Printer.WriteLine("Permission: " + p);
                //});

                // Filter
                var matchs = permissions.Where(p => p.Match(function, discriminators)).ToArray();
                Printer.Indent("Filtered permissions:", () => matchs.Print(PrintMode.Table));
                //Printer.WriteLine($"'{matchs.Length}' permission matches");

                return matchs;
            });
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
        public static IRolCan Can(this IRol me, params IFunction[] functions) => new _RolCan(me, functions, false);
        public static IRolCan EnsureCan(this IRol me, params IFunction[] functions) => new _RolCan(me, functions, true);
        public static bool IsRoot(this IRol me) => me.Can(Functions.Admin).Anything();
        #endregion
        #region Something
        public static bool Anything(this IRolCan me)
        {
            var mee = (IInternalRolCan)me;
            if (mee.Rol == null) return false;
            foreach (var fun in mee.Functions)
            {
                var permissions = mee.Rol.SearchPermissions(fun);
                if (!permissions.Any() || permissions.Any(p => p.Scopes.Any()))
                    return false;
            }
            return true;
        }
        public static bool Something(this IRolCan me)
        {
            var mee = (IInternalRolCan)me;
            if (mee.Rol == null) return false;
            foreach (var fun in mee.Functions)
            {
                var permissions = mee.Rol.SearchPermissions(fun);
                if (!permissions.Any(p => p.Value))// || permissions.All(p => p.Scopes.Any()))
                    return false;
            }
            return true;
        }
        #endregion
        #region Can().By..<IDiscriminator>()
        private static bool ByAll(this IRolCan me, params IDiscriminator[] discriminators)
            //where TDiscriminator : IDiscriminator
            => CheckDiscriminators((IInternalRolCan)me, true, discriminators);
        private static bool ByAny(this IRolCan me, params IDiscriminator[] discriminators)
            //where TDiscriminator : IDiscriminator
            => CheckDiscriminators((IInternalRolCan)me, false, discriminators);
        private static bool CheckDiscriminators(this IInternalRolCan me, bool forAll, params IDiscriminator[] discriminators)
        {
            if (me.Rol == null) return false;
            foreach (var fun in me.Functions)
            {
                var res = forAll
                    ? discriminators.All(dis =>
                     {
                         var pers = me.Rol.SearchPermissions(fun, dis);
                         return !pers.Any(p => !p.Value && p.Scopes.Any(s => dis.TypeId == s.Discriminator.TypeId)) && pers.Any(p => p.Value);
                     })
                    : discriminators.Any(dis =>
                     {
                         var pers = me.Rol.SearchPermissions(fun, dis);
                         return !pers.Any(p => !p.Value && p.Scopes.Any(s => dis.TypeId == s.Discriminator.TypeId)) && pers.Any(p => p.Value);
                     });
                if (!res)
                {
                    if (me.ThrowExceptionIfCannot)
                        throw new UnauthorizedAccessException($"The rol '{me.Rol.Name}' cannot '{me.Functions.Aggregate("", (a, c) => a + c.Name + "·", a => a.Trim('·'))}' for the given discriminators '{discriminators.Aggregate("", (a, c) => $"{a}, {c.Name}", a => a.Trim(',', ' ')) }'");
                    return false;
                }
            }
            return true;
        }
        #endregion
        #region Can().Type's
        // Only one type
        public static bool Type(this IRolCan me, Type type) => me.AllTypes(type);
        public static bool Type<T>(this IRolCan me) => me.AllTypes(typeof(T));
        // Two types
        public static bool AllTypes<T1, T2>(this IRolCan me) => me.AllTypes(typeof(T1), typeof(T2));
        public static bool AnyType<T1, T2>(this IRolCan me) => me.AnyType(typeof(T1), typeof(T2));
        // Three types
        public static bool AllTypes<T1, T2, T3>(this IRolCan me) => me.AllTypes(typeof(T1), typeof(T2), typeof(T3));
        public static bool AnyType<T1, T2, T3>(this IRolCan me) => me.AnyType(typeof(T1), typeof(T2), typeof(T3));
        // Many types
        public static bool AllTypes(this IRolCan me, params Type[] types) => me.ByAll(types.Select(t => Factory.Get<TypeDiscriminatorFactory>().FromType(t)).RemoveNulls().ToArray());
        public static bool AnyType(this IRolCan me, params Type[] types) => me.ByAny(types.Select(t => Factory.Get<TypeDiscriminatorFactory>().FromType(t)).RemoveNulls().ToArray());
        #endregion
        #region Can().Instance's
        // One instance
        public static bool Instance<T>(this IRolCan me, T value) => me.AllInstances(new[] { value });
        // Many instances
        public static bool AllInstances<T>(this IRolCan me, IEnumerable<T> values) => ((IInternalRolCan)me).CheckInstances(true, values);
        public static bool AnyInstance<T>(this IRolCan me, IEnumerable<T> values) => ((IInternalRolCan)me).CheckInstances(false, values);
        // -------------------------- IMPLEMENTATION
        private static bool CheckInstances<T>(this IInternalRolCan me, bool forAll, IEnumerable<T> values)
        {
            if (me.Rol == null) return false;
            var res = forAll ? values.AuthorizedTo(me.Rol, me.Functions).Count() == values.Count() : values.AuthorizedTo(me.Rol, me.Functions).Any();
            if (me.ThrowExceptionIfCannot && !res)
                throw new UnauthorizedAccessException($"The rol '{me.Rol.Name}' cannot '{me.Functions.Aggregate("", (a, c) => a + c.Name + "·", a => a.Trim('·'))}' for the given instances '{values}'");
            return res;
        }
        #endregion
        #endregion
    }
    internal interface IInternalRolCan : IRolCan
    {
        IRol Rol { get; }
        IFunction[] Functions { get; }
        bool ThrowExceptionIfCannot { get; }
    }
    public interface IRolCan
    {
        //IRol Rol { get; }
        //IFunction[] Functions { get; }
        //bool ThrowExceptionIfCannot { get; }
        //bool IsFunctionAssigned(IFunction function, IDiscriminator[] discriminators);
    }
    class _RolCan : IInternalRolCan
    {
        public _RolCan(IRol rol, IFunction[] functions, bool throwExceptionIfCannot)
        {
            Rol = rol;
            Functions = functions;
            ThrowExceptionIfCannot = throwExceptionIfCannot;
        }
        public IRol Rol { get; set; }
        public IFunction[] Functions { get; set; }
        //public bool IsFunctionAssigned(IFunction function, IDiscriminator[] discriminators)
        //{
        //    return Printer.Indent($"{nameof(_RolCan)}.{nameof(IsFunctionAssigned)}:", () =>
        //    {
        //        var pers = Rol.SearchPermissions(function, discriminators);
        //        if (pers.Any(p => !p.Value))
        //        {
        //            // I have denegation permissions
        //            Printer.WriteLine($"Resultado: NO VALIDO - Denegado por un permiso");
        //            return false;
        //        }
        //        if (!pers.Any(p => p.Value))
        //        {
        //            // I haven't grant permissions
        //            Printer.WriteLine($"Resultado: NO VALIDO - Denegado por no encontrar un permiso de concesión");
        //            return false;
        //        }
        //        Printer.WriteLine($"Resultado: VALIDO");
        //        return true;
        //    });
        //}
        public bool ThrowExceptionIfCannot { get; set; }
    }
}
