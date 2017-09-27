using Fuxion.Factories;
using static Fuxion.Identity.Helpers.Comparer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity
{
    internal static class IdentityExtensions
    {
        internal static IEnumerable<IPermission> AllPermissions(this IRol me)
        {
            var r = new List<IPermission>();
            if (me.Permissions != null) r.AddRange(me.Permissions);
            if (me.Groups != null) foreach (var gro in me.Groups) r.AddRange(gro.AllPermissions());
            return r;
        }
        internal static IPermission[] SearchPermissions(this IRol me, bool forFilter, IFunction function, TypeDiscriminator typeDiscriminator, params IDiscriminator[] discriminators)
        {
            using(var res = Printer.CallResult<IPermission[]>())
            {
                discriminators = discriminators.RemoveNulls();
                using (Printer.Indent2("Inpupt Prameters"))
                {
                    Printer.WriteLine($"Rol: {me?.Name}");
                    Printer.WriteLine($"For filter: " + forFilter);
                    Printer.WriteLine($"Function: {function?.ToString() ?? "<null>"}");
                    Printer.WriteLine($"Type discriminator: {typeDiscriminator?.ToString() ?? "<null>"}");
                    Printer.Foreach($"Discriminators:", discriminators, dis => Printer.WriteLine($"{dis}"));
                }
                // Function validation
                if (!(function?.IsValid() ?? true)) throw new ArgumentException($"The '{nameof(function)}' pararameter with value '{function}' has an invalid state", nameof(function));

                // TypeDiscriminator validation
                if (!typeDiscriminator?.IsValid() ?? false) throw new InvalidStateException($"The '{typeDiscriminator}' discriminator has an invalid state");

                // Discriminators validation
                var invalidDiscriminator = discriminators?.FirstOrDefault(d => !d.IsValid());

                if (invalidDiscriminator != null) throw new InvalidStateException($"The '{invalidDiscriminator}' discriminator has an invalid state");

                // Get & print rol permissions
                var permissions = me.AllPermissions();
                using (Printer.Indent2("Permissions:"))
                    permissions.Print(PrintMode.Table);
                using (Printer.Indent2("Iterate permissions:"))
                    res.Value = permissions.Where(p => p.Match(forFilter, function, typeDiscriminator, discriminators)).ToArray();
                res.OnPrintResult = r => r.Print(PrintMode.Table);
                return res.Value;
            }
        }
        internal static bool CheckDiscriminators(this IInternalRolCan me, bool forAll, TypeDiscriminator typeDiscriminator, params IDiscriminator[] discriminators)
        {
            using(var res = Printer.CallResult<bool>())
            {
                using (Printer.Indent2("Input parameters"))
                {
                    Printer.WriteLine($"Rol: {me?.Rol?.Name}");
                    Printer.WriteLine($"Functions: {string.Join(",", me.Functions.Select(f => f.Name)) ?? "<null>"}");
                    Printer.WriteLine($"For all: {forAll}");
                    Printer.WriteLine($"Type discriminator: {typeDiscriminator?.ToString() ?? "null"}");
                    Printer.Foreach($"Discriminators:", discriminators, dis => Printer.WriteLine($"{dis}"));
                }
                // If Rol is null, return false
                if (me.Rol == null)
                {
                    Printer.WriteLine($"'{nameof(me.Rol)}' is NULL");
                    return res.Value = false;
                }
                // If target discriminator is null, return true
                if (typeDiscriminator == null)
                {
                    Printer.WriteLine($"'{nameof(typeDiscriminator)}' is NULL");
                    return res.Value = true;
                }
                bool Compute()
                {
                    //Printer.Foreach("Iterating functions:", me.Functions, fun => {
                    foreach (var fun in me.Functions)
                    {
                        Printer.WriteLine($"Function '{fun.Name}':");
                        var pers = SearchPermissions(me.Rol, false, fun, typeDiscriminator, discriminators);
                        if (!pers.Any())
                            return false;
                        else
                        {
                            var grantPermissions = pers.Where(p => p.Value).ToList();
                            var deniedPermissions = pers.Where(p => !p.Value).ToList();
                            Printer.WriteLine($"Found '{grantPermissions.Count}' grant permissions");
                            Printer.WriteLine($"Found '{deniedPermissions.Count}' denied permissions");
                            bool r = false;
                            if (discriminators.IsNullOrEmpty())
                            {
                                r = grantPermissions.Count > 0 && deniedPermissions.Count == 0;
                            }
                            else
                            {
                                r = forAll
                                    ? discriminators.All(dis =>
                                    {
                                        return grantPermissions.Count > 0 && deniedPermissions.Count == 0;// || grantPermissions.Count == 0;
                                    })
                                    : discriminators.Any(dis =>
                                    {
                                        //var pers = me.Rol.SearchPermissions(fun, dis);
                                        //return !pers.Any(p => !p.Value && p.Scopes.Any(s => dis.TypeId == s.Discriminator.TypeId)) && pers.Any(p => p.Value);
                                        return !pers.Any(p => !p.Value && p.Match(false, fun, typeDiscriminator, discriminators)) && pers.Any(p => p.Value);
                                    });
                            }
                            if (!r && me.ThrowExceptionIfCannot)
                                throw new UnauthorizedAccessException($"The rol '{me.Rol.Name}' cannot '{me.Functions.Aggregate("", (a, c) => a + c.Name + "·", a => a.Trim('·'))}' of type '{typeDiscriminator.Name}' with given discriminators '{discriminators.Aggregate("", (a, c) => $"{a}, {c.TypeName + "<" + c.Name + ">"}", a => a.Trim(',', ' ')) }'");
                            return r;
                        }
                    }
                    return false;
                }
                res.Value = Compute();
                if (!res.Value && me.ThrowExceptionIfCannot)
                    throw new UnauthorizedAccessException($"The rol '{me.Rol.Name}' cannot '{me.Functions.Aggregate("", (a, c) => a + c.Name + "·", a => a.Trim('·'))}' of type '{typeDiscriminator.Name}' with given discriminators '{discriminators.Aggregate("", (a, c) => $"{a}, {c.TypeName + "<" + c.Name + ">"}", a => a.Trim(',', ' ')) }'");
                return res.Value;
            }
        }
        internal static IEnumerable<(PropertyInfo PropertyInfo, Type PropertyType, Type DiscriminatorType, object DiscriminatorTypeId)> GetDiscriminatedProperties(this Type me)
        {
            using (var res = Printer.CallResult<IEnumerable<(PropertyInfo PropertyInfo, Type PropertyType, Type DiscriminatorType, object DiscriminatorTypeId)>>())
            {
                res.OnPrintResult = r =>
                {
                    foreach (var p in r)
                        Printer.WriteLine($"Property '{p.PropertyInfo.Name}' of type '{p.PropertyType.Name}' is discriminated by '{p.DiscriminatorTypeId.ToString()}' of type '{p.DiscriminatorType.Name}'");
                };
                return res.Value = me.GetRuntimeProperties()
                   .Where(p => p.GetCustomAttribute<DiscriminatedByAttribute>(true, false, false) != null)
                   .Select(p => (
                       PropertyInfo: p,
                       PropertyType: p.PropertyType,
                       DiscriminatorType: p.GetCustomAttribute<DiscriminatedByAttribute>(true).Type,
                       DiscriminatorTypeId:
                           p.GetCustomAttribute<DiscriminatedByAttribute>(true).Type.GetTypeInfo()
                               .GetCustomAttribute<DiscriminatorAttribute>(true).TypeId));
            }
        }
        internal static IEnumerable<IDiscriminator> GetDiscriminatorsOfDiscriminatedProperties(this Type me, object value = null)
        =>
            me.GetDiscriminatedProperties().Select(p =>
             {
                 object val = null;
                 if (value != null)
                     val = p.PropertyInfo.GetValue(value);
                 if (val == null) return Discriminator.Empty(p.DiscriminatorType);
                 return Discriminator.ForId(p.DiscriminatorType, val);
             }).RemoveNulls();
        internal static Expression<Func<TEntity, bool>> FilterExpression<TEntity>(this IRol me, IFunction[] functions)
        {
            using(var res = Printer.CallResult<Expression<Func<TEntity, bool>>>())
            {
                using (Printer.Indent2("Input parameters:"))
                {
                    Printer.WriteLine("Rol:");
                    new[] { me }.Print(PrintMode.Table);
                    Printer.WriteLine("Functions:");
                    functions.Print(PrintMode.Table);
                    Printer.WriteLine("Type: " + typeof(TEntity).Name);
                }
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
                Expression<Func<TEntity, bool>> GetContainsExpression(bool value, IScope sco, Type disType, PropertyInfo proInfo)
                {
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
                var props = typeof(TEntity).GetDiscriminatedProperties();
                Printer.Foreach("Properties:", props, p => Printer.WriteLine($"{p.PropertyType.Name} {p.PropertyInfo.Name} - {p.DiscriminatorTypeId} {p.DiscriminatorType.Name}"));
                var pers = functions.SelectMany(fun => me.SearchPermissions(
                    true,
                    fun,
                    Factory.Get<TypeDiscriminatorFactory>().FromType<TEntity>(),
                    typeof(TEntity).GetDiscriminatorsOfDiscriminatedProperties().ToArray()
                    ))
                .Distinct().ToList();
                Expression<Func<TEntity, bool>> denyPersExp = null;
                Printer.Foreach("Deny permissions:", pers.Where(p => !p.Value), per =>
                {
                    Expression<Func<TEntity, bool>> perExp = null;
                    using (Printer.Indent2($"Permission: {per.ToOneLineString()}"))
                    {
                        Printer.Foreach("Scopes:", per.Scopes, sco =>
                        {
                            using (Printer.Indent2($"Scope: {sco.ToOneLineString()}"))
                            {
                                // Recorro las propiedades que son del tipo de este discriminador
                                foreach (var pro in props.Where(p => AreEquals(p.DiscriminatorTypeId, sco.Discriminator.TypeId)).ToList())
                                {
                                    Printer.WriteLine($"Property: {pro.PropertyType.Name} {pro.PropertyInfo.Name}");
                                    var exp = GetContainsExpression(per.Value, sco, pro.DiscriminatorType, pro.PropertyInfo);
                                    if (exp == null) exp = Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(false), Expression.Parameter(typeof(TEntity)));
                                    if (perExp == null)
                                        perExp = exp;
                                    else
                                        perExp = perExp.And(exp);
                                }
                            }
                        });
                        if (perExp == null)
                        {
                            perExp = Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(false), Expression.Parameter(typeof(TEntity)));
                        }
                    }
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

                    using (Printer.Indent2($"Permission: {per.ToOneLineString()}"))
                    {
                        Expression<Func<TEntity, bool>> perExp = null;
                        Printer.Foreach("Scopes:", per.Scopes, sco =>
                        {
                            using (Printer.Indent2($"Scope: {sco.ToOneLineString()}"))
                            {
                                // Recorro las propiedades que son del tipo de este discriminador
                                foreach (var pro in props.Where(p => AreEquals(p.DiscriminatorTypeId, sco.Discriminator.TypeId)).ToList())
                                {
                                    Printer.WriteLine($"Property: {pro.PropertyType.Name} {pro.PropertyInfo.Name}");
                                    var exp = GetContainsExpression(per.Value, sco, pro.DiscriminatorType, pro.PropertyInfo);
                                    if (exp == null) exp = Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(false), Expression.Parameter(typeof(TEntity)));
                                    if (perExp == null)
                                        perExp = exp;
                                    else
                                        perExp = perExp.Or(exp);
                                }
                            }
                        });
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
                    }
                });
                if (denyPersExp != null && grantPersExp != null) res.Value = denyPersExp.And(grantPersExp);
                if (denyPersExp != null && grantPersExp == null) res.Value = denyPersExp;
                if (denyPersExp == null && grantPersExp != null) res.Value = grantPersExp;
                res.Value = res.Value ?? Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(false), Expression.Parameter(typeof(TEntity)));
                #region PrintExpression
                void PrintExpression(Expression exp)
                {
                    if (exp == null) Printer.WriteLine("NULL");
                    else if (exp is BinaryExpression)
                        PrintBinaryExpression(exp as BinaryExpression);
                    else if (exp is MethodCallExpression)
                        PrintMethodCallExpression(exp as MethodCallExpression);
                    else if (exp is ConstantExpression)
                        PrintConstantExpression(exp as ConstantExpression);
                    else if (exp is MemberExpression)
                        PrintMemberExpression(exp as MemberExpression);
                    else if (exp is UnaryExpression)
                        PrintUnaryExpression(exp as UnaryExpression);
                    else
                        Printer.WriteLine($"'{exp.GetType().Name}'");
                }
                void PrintBinaryExpression(BinaryExpression exp)
                {
                    using (Printer.Indent2("("))
                    {
                        PrintExpression(exp.Left);
                        if (Printer.IsLineWritePending) Printer.WriteLine("");
                        Printer.WriteLine(exp.NodeType.ToString().ToUpper());
                        PrintExpression(exp.Right);
                        if (Printer.IsLineWritePending) Printer.WriteLine("");
                    }
                    if (Printer.IsLineWritePending) Printer.WriteLine("");
                    Printer.WriteLine(")");
                }
                void PrintMethodCallExpression(MethodCallExpression exp)
                {
                    var isExtenssionMethod = exp.Method.GetCustomAttribute<ExtensionAttribute>() != null;
                    if (isExtenssionMethod)
                    {
                        PrintExpression(exp.Arguments[0]);
                        Printer.Write($".{exp.Method.Name}(");
                        foreach (var e in exp.Arguments.Skip(1))
                            PrintExpression(e);
                    }
                    else
                    {
                        Printer.Write($"{exp.Method.Name}(");
                        foreach (var e in exp.Arguments)
                            PrintExpression(e);
                    }
                    Printer.Write(")");
                }
                void PrintConstantExpression(ConstantExpression exp)
                {
                    string r = "";
                    if (exp.Value == null)
                        r += "null";
                    else if (exp.Value.GetType().IsSubclassOfRawGeneric(typeof(List<>)))
                    {
                        string toAdd = "[";
                        foreach (var obj in (IEnumerable)exp.Value)
                            toAdd += obj + ", ";
                        toAdd = toAdd.Trim(' ', ',') + "]";
                        r += toAdd;
                    }
                    else
                        r += exp.Value;
                    Printer.Write(r);
                }
                void PrintMemberExpression(MemberExpression exp)
                {
                    string r = "";
                    r += exp.Member.Name;
                    Printer.Write(r);
                }
                void PrintUnaryExpression(UnaryExpression exp)
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
                #endregion
                res.OnPrintResult = r =>
                {
                    Printer.WriteLine("Expression:");
                    PrintExpression(r?.Body);
                    Printer.WriteLine("");
                };
                return res.Value;
            }
        }
    }
}
