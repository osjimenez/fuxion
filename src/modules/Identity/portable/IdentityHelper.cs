using Fuxion.Factories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity
{
    internal class IdentityHelper
    {
        internal static IPermission[] SearchPermissions(IRol me, IFunction function = null, params IDiscriminator[] discriminators)
        {
            discriminators = discriminators.RemoveNulls();
            return Printer.Indent($"{nameof(RolExtensions)}.{nameof(SearchPermissions)}:", () =>
            {
                Printer.Indent("Input parameters", () =>
                {
                    Printer.WriteLine($"Rol: {me?.Name}");
                    Printer.WriteLine($"Function: {function?.Name ?? "<null>"}");
                    Printer.Foreach($"Discriminators:", discriminators, dis => Printer.WriteLine($"{dis?.TypeName} - {dis?.Name}"));
                });
                // Function validation
                if (!(function?.IsValid() ?? true)) throw new ArgumentException($"The '{nameof(function)}' pararameter with value '{function}' has an invalid state", nameof(function));
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
        internal static bool CheckDiscriminators(IInternalRolCan me, bool forAll, params IDiscriminator[] discriminators)
        {
            discriminators = discriminators.RemoveNulls();
            return Printer.Indent($"{nameof(RolExtensions)}.{nameof(CheckDiscriminators)}:", () =>
            {
                Printer.Indent("Input parameters", () =>
                {
                    Printer.WriteLine($"Rol: {me?.Rol?.Name}");
                    Printer.WriteLine($"Functions: {string.Join(",", me.Functions.Select(f => f.Name)) ?? "<null>"}");
                    Printer.Foreach($"Discriminators:", discriminators, dis => Printer.WriteLine($"{dis?.TypeName} - {dis?.Name}"));
                });
                if (me.Rol == null) return false;
                bool res = true;
                Printer.Foreach("Iterating functions:", me.Functions, fun => {
                    Printer.WriteLine($"Function '{fun.Name}':");
                    var pers = SearchPermissions(me.Rol, fun, discriminators);
                    var discriminatorsTypeIds = discriminators.Select(d => d.TypeId);
                    var r = forAll
                        ? discriminators.All(dis =>
                        {
                            //var pers = me.Rol.SearchPermissions(fun, dis);
                            var grantPermissions = pers.Where(p => p.Value).ToList();
                            var deniedPermissions = pers.Where(p => !p.Value).ToList();
                            //var deniedMatchedPermissions = deniedPermissions.Where(p => p.Match(fun, discriminators)).ToList();
                            Printer.WriteLine($"Found '{grantPermissions.Count}' grant permissions");
                            Printer.WriteLine($"Found '{deniedPermissions.Count}' denied permissions");
                            //Printer.WriteLine($"Found '{deniedMatchedPermissions.Count}' denied matched permissions");
                            //return !pers.Any(p => !p.Value && p.Match(fun, discriminators)) && pers.Any(p => p.Value);
                            //return grantPermissions.Count > 0 && deniedMatchedPermissions.Count <= 0;
                            return grantPermissions.Count > 0 && deniedPermissions.Count <= 0;
                        })
                        : discriminators.Any(dis =>
                        {
                            //var pers = me.Rol.SearchPermissions(fun, dis);
                            //return !pers.Any(p => !p.Value && p.Scopes.Any(s => dis.TypeId == s.Discriminator.TypeId)) && pers.Any(p => p.Value);
                            return !pers.Any(p => !p.Value && p.Match(fun, discriminators)) && pers.Any(p => p.Value);
                        });
                    if (!r)
                    {
                        if (me.ThrowExceptionIfCannot)
                            throw new UnauthorizedAccessException($"The rol '{me.Rol.Name}' cannot '{me.Functions.Aggregate("", (a, c) => a + c.Name + "·", a => a.Trim('·'))}' for the given discriminators '{discriminators.Aggregate("", (a, c) => $"{a}, {c.Name}", a => a.Trim(',', ' ')) }'");
                        res = false;
                    }
                });
                //foreach (var fun in me.Functions)
                //{
                //    var pers = SearchPermissions(me.Rol, fun, discriminators);
                //    var discriminatorsTypeIds = discriminators.Select(d => d.TypeId);
                //    var res = forAll
                //        ? discriminators.All(dis =>
                //        {
                //            //var pers = me.Rol.SearchPermissions(fun, dis);
                //            var grantPermissions = pers.Where(p => p.Value).ToList();
                //            var deniedPermissions = pers.Where(p => !p.Value).ToList();
                //            //var deniedMatchedPermissions = deniedPermissions.Where(p => p.Match(fun, discriminators)).ToList();
                //            Printer.WriteLine($"Found '{grantPermissions.Count}' grant permissions");
                //            Printer.WriteLine($"Found '{deniedPermissions.Count}' denied permissions");
                //            //Printer.WriteLine($"Found '{deniedMatchedPermissions.Count}' denied matched permissions");
                //            //return !pers.Any(p => !p.Value && p.Match(fun, discriminators)) && pers.Any(p => p.Value);
                //            //return grantPermissions.Count > 0 && deniedMatchedPermissions.Count <= 0;
                //            return grantPermissions.Count > 0 && deniedPermissions.Count <= 0;
                //        })
                //        : discriminators.Any(dis =>
                //        {
                //            //var pers = me.Rol.SearchPermissions(fun, dis);
                //            //return !pers.Any(p => !p.Value && p.Scopes.Any(s => dis.TypeId == s.Discriminator.TypeId)) && pers.Any(p => p.Value);
                //            return !pers.Any(p => !p.Value && p.Match(fun, discriminators)) && pers.Any(p => p.Value);
                //        });
                //    if (!res)
                //    {
                //        if (me.ThrowExceptionIfCannot)
                //            throw new UnauthorizedAccessException($"The rol '{me.Rol.Name}' cannot '{me.Functions.Aggregate("", (a, c) => a + c.Name + "·", a => a.Trim('·'))}' for the given discriminators '{discriminators.Aggregate("", (a, c) => $"{a}, {c.Name}", a => a.Trim(',', ' ')) }'");
                //        return false;
                //    }
                //}
                return res;
            });
        }
        internal static Expression<Func<TEntity, bool>> FilterExpression<TEntity>(IRol me, IFunction[] functions)
        {
            return Printer.Indent($"{typeof(IdentityHelper).GetTypeInfo().DeclaredMethods.FirstOrDefault(m => m.Name == nameof(FilterExpression)).GetSignature()}:", () =>
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
                var pers = functions.SelectMany(fun => SearchPermissions(me, fun, Factory.Get<TypeDiscriminatorFactory>().FromType<TEntity>())).Distinct().ToList();
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

                    Printer.Indent($"Permission: {per}>", () =>
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

                Printer.WriteLine("Expression:");
                PrintExpression(res?.Body);
                if (Printer.IsLineWritePending) Printer.WriteLine("");

                return res;
            });
        }
    }
}
