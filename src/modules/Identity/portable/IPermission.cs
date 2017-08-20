﻿using System.Collections.Generic;
using System.Linq;
using Fuxion.Identity.Helpers;
using System.Reflection;
using Fuxion.Factories;
using System.Diagnostics;

namespace Fuxion.Identity
{
    public interface IPermission
    {
        IFunction Function { get; }
        IEnumerable<IScope> Scopes { get; }
        bool Value { get; }
    }
    public static class PermissionExtensions
    {
        public static bool IsValid(this IPermission me) { return me.Function != null && (me.Scopes?.All(s => s.Discriminator != null) ?? true) && me.Scopes?.Select(s => s.Discriminator.TypeId).Distinct().Count() == me.Scopes?.Count(); }
        internal static bool Match2(this IPermission me, IFunction function, IDiscriminator targetDiscriminator, params IDiscriminator[] discriminators)
        {
            using (Printer.Indent2($"{nameof(PermissionExtensions)}.{nameof(Match2)}:"))
            {
                using (Printer.Indent2("Input parameters"))
                {
                    Printer.WriteLine($"Permission:");
                    new[] { me }.Print(PrintMode.Table);
                    Printer.WriteLine($"Function: {function?.Name ?? "<null>"}");

                    if (targetDiscriminator != null)
                    {
                        Printer.WriteLine($"Target discriminator:");
                        new[] { targetDiscriminator }.Print(PrintMode.Table);
                    }else Printer.WriteLine($"Target discriminator: null");
                    Printer.WriteLine($"Discriminators:");
                    discriminators.Print(PrintMode.Table);
                }
                if (function == null || !me.MatchByFunction(function))
                {
                    Printer.WriteLine($"Matching failed on check the function");
                    return false;
                }
                if (!me.MatchByDiscriminatorsInclusionsAndExclusions2(targetDiscriminator, discriminators))
                {
                    Printer.WriteLine($"Matching failed on check the inclusions/exclusions of discriminator");
                    return false;
                }
                return true;
            }
        }
        internal static bool Match(this IPermission me, IFunction function = null, params IDiscriminator[] discriminators)
        {
            using (Printer.Indent2($"{nameof(PermissionExtensions)}.{nameof(Match)}:"))
            {
                using (Printer.Indent2("Input parameters"))
                {
                    Printer.WriteLine($"Permission:");
                    new[] { me }.Print(PrintMode.Table);
                    Printer.WriteLine($"Function: {function?.Name ?? "<null>"}");
                    Printer.WriteLine($"Discriminators:");
                    discriminators.Print(PrintMode.Table);
                }
                if (function == null || me.MatchByFunction(function))
                {
                    if (discriminators == null || !discriminators.Any() || me.MatchByDiscriminatorsType(discriminators))
                    {
                        if (discriminators == null || !discriminators.Any() || me.MatchByDiscriminatorsInclusionsAndExclusions(discriminators))
                        {
                            return true;
                        }
                        else
                        {
                            Printer.WriteLine($"Matching failed on check the inclusions/exclusions of discriminator");
                            return false;
                        }
                    }
                    else
                    {
                        Printer.WriteLine($"Matching failed on check the type of discriminator");
                        return false;
                    }
                }
                else
                {
                    Printer.WriteLine($"Matching failed on check the function");
                    return false;
                }
            }
        }
        internal static bool MatchByFunction(this IPermission me, IFunction function)
        {
            using (Printer.Indent2($"{nameof(PermissionExtensions)}.{nameof(MatchByFunction)}:"))
            {
                using (Printer.Indent2("Input parameters"))
                {
                    Printer.WriteLine($"Permission:");
                    new[] { me }.Print(PrintMode.Table);
                    Printer.WriteLine($"Function: {function.Name}");
                }
                Printer.WriteLine($"Inclusiones: {me.Function.GetAllInclusions().Aggregate("", (a, s) => a + " - " + s.Id, a => a.Trim(' ', '-'))}");
                Printer.WriteLine($"Exclusiones: {me.Function.GetAllExclusions().Aggregate("", (a, s) => a + " - " + s.Id, a => a.Trim(' ', '-'))}");
                var comparer = new FunctionEqualityComparer();
                bool res = false;
                if (comparer.Equals(me.Function, function))
                {
                    Printer.WriteLine("Match with same function");
                    res = true;
                }
                else if (me.Value && me.Function.GetAllInclusions().Contains(function, comparer))
                {
                    Printer.WriteLine("Match by included function");
                    res = true;
                }
                else if (!me.Value && me.Function.GetAllExclusions().Contains(function, comparer))
                {
                    Printer.WriteLine("Match by excluded function");
                    res = true;
                }
                Printer.WriteLine($"RESULT: {res}");
                return res;
            }
        }
        internal static bool MatchByDiscriminatorsType(this IPermission me, params IDiscriminator[] discriminators)
        {
            return true;
            //using (Printer.Indent2($"{nameof(PermissionExtensions)}.{nameof(MatchByDiscriminatorsType)}:"))
            //{
            //    using (Printer.Indent2("Input parameters"))
            //    {
            //        Printer.WriteLine("Permission:");
            //        new[] { me }.Print(PrintMode.Table);
            //        Printer.WriteLine("Discriminators:");
            //        discriminators.Print(PrintMode.Table);
            //    }
            //    bool res = false;
            //    Printer.WriteLine("Or my permission haven't scopes or some of these scopes match by discriminator type with any of given discriminators.");
            //    if (me.Scopes.Count() == 0)
            //    {
            //        Printer.WriteLine($"This permission hasn't any scope");
            //        //res = me.Value;
            //        res = true;
            //    }
            //    else
            //    {
            //        var scos = me.Scopes.Where(s => discriminators.Select(d => d.TypeId).Contains(s.Discriminator.TypeId)).ToList();
            //        Printer.WriteLine($"This permission has '{scos.Count()}' scopes for type of given discriminators");
            //        if(scos.Count == 0)
            //        {

            //            res = true;
            //        }
            //        else
            //        {
            //            res = true;
            //        }

                    

            //        //res = scos.Count() > 0 ? !me.Value : true;

            //        //var typeDis = discriminators.FirstOrDefault(d => d.TypeId.ToString() == TypeDiscriminator.TypeDiscriminatorId);
            //        //if (typeDis != null) using (Printer.Indent2($"Has a discriminator 'TypeDiscriminator' = '{typeDis}'"))
            //        //    {
            //        //        var types = Factory.Get<TypeDiscriminatorFactory>().TypesFromId(typeDis.Id.ToString());
            //        //        foreach(var type in types)
            //        //        {
            //        //            if(!type.GetRuntimeProperties()
            //        //               .Where(p => p.GetCustomAttribute<DiscriminatedByAttribute>(true, false, false) != null)
            //        //               .Select(p => new
            //        //               {
            //        //                   PropertyInfo = p,
            //        //                   PropertyType = p.PropertyType,
            //        //                   DiscriminatorType = p.GetCustomAttribute<DiscriminatedByAttribute>(true).Type,
            //        //                   DiscriminatorTypeId =
            //        //                       p.GetCustomAttribute<DiscriminatedByAttribute>(true).Type.GetTypeInfo()
            //        //                           .GetCustomAttribute<DiscriminatorAttribute>(true).TypeId,
            //        //               })
            //        //               .Any(p => me.Scopes.Select(s => s.Discriminator.TypeId).Contains(p.DiscriminatorTypeId)))
            //        //            {
            //        //                Debug.WriteLine("");
            //        //            }
            //        //        }
            //        //        //((TypeDiscriminator)typeSco.Discriminator)
            //        //    }
            //        //else res = true;
            //    }
            //    Printer.WriteLine($"RESULT: {res}");
            //    return res;
            //}
        }
        internal static bool MatchByDiscriminatorsInclusionsAndExclusions2(this IPermission me, IDiscriminator targetDiscriminator, params IDiscriminator[] discriminators)
        {
            using(Printer.Indent2($"{typeof(PermissionExtensions).GetTypeInfo().DeclaredMethods.FirstOrDefault(m => m.Name == nameof(MatchByDiscriminatorsInclusionsAndExclusions2)).GetSignature()}:"))
            {
                using (Printer.Indent2("Input parameters"))
                {
                    Printer.WriteLine("Permission:");
                    new[] { me }.Print(PrintMode.Table);
                    if (targetDiscriminator == null)
                        Printer.WriteLine($"Target discriminator: null");
                    else
                    {
                        Printer.WriteLine($"Target discriminator:");
                        new[] { targetDiscriminator }.Print(PrintMode.Table);
                    }
                    Printer.WriteLine("Discriminators:");
                    discriminators.Print(PrintMode.Table);
                }
                var res = false;
                // Si el permiso no define scopes, TRUE
                if (!me.Scopes.Any()) res = true;
                // Si el discriminador objetivo es null, TRUE
                if (targetDiscriminator == null) res = true;
                // Si no es null y el target es TypeDiscriminator, si no hay TypeDiscriminator en el permiso, TRUE
                else if (Comparer.AreEquals(targetDiscriminator.TypeId, TypeDiscriminator.TypeDiscriminatorId) && !me.Scopes.Any(s => Comparer.AreEquals(s.Discriminator.TypeId, TypeDiscriminator.TypeDiscriminatorId)))
                    res = true;
                // Si alguno de los discriminadores esta relacionado con los scopes del permiso, TRUE
                if (discriminators.Any(d => me.Scopes.Any(s => s.Discriminator.GetAllRelated(s.Propagation).Any(d2 => Comparer.AreEquals(d.Id,d2.Id)))))
                    res = true;
                // Si el discriminador objetivo esta relacionado con alguno de los scopes del permiso, TRUE
                if (me.Scopes.Any(s => s.Discriminator.GetAllRelated(s.Propagation).Any(d => Comparer.AreEquals(d.Id, targetDiscriminator?.Id))))
                    res = true;
                
                Printer.WriteLine($"RESULT: {res}");
                return res;
            }
        }
        internal static bool MatchByDiscriminatorsInclusionsAndExclusions(this IPermission me, params IDiscriminator[] discriminators)
        {
            using (Printer.Indent2($"{typeof(PermissionExtensions).GetTypeInfo().DeclaredMethods.FirstOrDefault(m => m.Name == nameof(MatchByDiscriminatorsInclusionsAndExclusions)).GetSignature()}:"))
            {
                using (Printer.Indent2("Input parameters"))
                {
                    Printer.WriteLine("Permission:");
                    new[] { me }.Print(PrintMode.Table);
                    Printer.WriteLine("Discriminators:");
                    discriminators.Print(PrintMode.Table);
                }
                bool res = false;
                if (!me.Scopes.Any())
                {
                    Printer.WriteLine($"Haven't scopes");
                    res = true;
                }
                else
                {
                    using (Printer.Indent2("Analyze each scope:"))
                    {
                        // Tenemos que tomar nuestros discriminadores, y comprobarlos contra los discriminadores que me han pasado
                        // - Cojo un discriminador y busco el discriminador del mismo tipo en la entrada:
                        //    - No hay un discriminador del mismo tipo, pues no encaja
                        //    - Si hay un discriminador del mismo tipo, compruebo la ruta
                        var result = new List<bool?>();
                        //var result = me.Scopes.Select<IScope, bool?>(sco => {
                        foreach(var sco in me.Scopes)
                        { 
                            Printer.WriteLine($"Scope {sco}");
                            Printer.IndentationLevel++;
                            if (discriminators.Count(d => Comparer.AreEquals(d.TypeId, sco.Discriminator.TypeId)) == 1)
                            {
                                // Si hay un discriminador del mismo tipo, compruebo la ruta
                                var target = discriminators.Single(d => Comparer.AreEquals(d.TypeId, sco.Discriminator.TypeId));
                                Printer.WriteLine($"Se propaga a mi {sco.Propagation.HasFlag(ScopePropagation.ToMe)} ids = {target.Id}-{sco.Discriminator.Id}");
                                // Se propaga a mi y es el mismo discriminador
                                if (sco.Propagation.HasFlag(ScopePropagation.ToMe) && Comparer.AreEquals(target.Id, sco.Discriminator.Id))
                                {
                                    Printer.WriteLine($"Se propaga a mi y es el mismo discriminador");
                                    result.Add(true);
                                }
                                // Se propaga hacia arriba y su id esta en mi path:
                                else if (sco.Propagation.HasFlag(ScopePropagation.ToExclusions) && sco.Discriminator.GetAllExclusions().Contains(target))
                                {
                                    Printer.WriteLine($"Se propaga hacia arriba y su id esta en mi path");
                                    result.Add(true);
                                }
                                // Se propaga hacia abajo y mi id esta en su path:
                                else if (sco.Propagation.HasFlag(ScopePropagation.ToInclusions) && sco.Discriminator.GetAllInclusions().Contains(target))
                                {
                                    Printer.WriteLine($"Se propaga hacia abajo y mi id esta en su path");
                                    result.Add(true);
                                }
                                else
                                {
                                    Printer.IndentationLevel--;
                                    result.Add(false);
                                }
                            }
                            else
                            {
                                // No hay un discriminador del mismo tipo, pues no encaja
                                Printer.WriteLine($"No hay un discriminador del mismo tipo");



                                //var typeDis = discriminators.FirstOrDefault(d => d.TypeId.ToString() == TypeDiscriminator.TypeDiscriminatorId);
                                //if (typeDis != null) using (Printer.Indent2($"Has a discriminator 'TypeDiscriminator' = '{typeDis}'"))
                                //    {
                                //        var types = Factory.Get<TypeDiscriminatorFactory>().TypesFromId(typeDis.Id.ToString());
                                //        foreach (var type in types)
                                //        {
                                //            var props = type.GetRuntimeProperties()
                                //               .Where(p => p.GetCustomAttribute<DiscriminatedByAttribute>(true, false, false) != null)
                                //               .Select(p => new
                                //               {
                                //                   PropertyInfo = p,
                                //                   PropertyType = p.PropertyType,
                                //                   DiscriminatorType = p.GetCustomAttribute<DiscriminatedByAttribute>(true).Type,
                                //                   DiscriminatorTypeId =
                                //                       p.GetCustomAttribute<DiscriminatedByAttribute>(true).Type.GetTypeInfo()
                                //                           .GetCustomAttribute<DiscriminatorAttribute>(true).TypeId,
                                //               })
                                //               .Where(p => me.Scopes.Select(s => s.Discriminator.TypeId).Contains(p.DiscriminatorTypeId));
                                //            {
                                //                Debug.WriteLine("");
                                //                if (props.Count() > 0)
                                //                {
                                //                    return false;
                                //                }
                                //            }
                                //        }
                                //        //((TypeDiscriminator)typeSco.Discriminator)
                                //        return true;
                                //    }
                                //else return true;



                                Printer.IndentationLevel--;
                                result.Add(null);
                            }
                        }
                        Printer.WriteLine($"results: {result.Aggregate("", (c, a) => c + ", " + (a == null ? "null" : a.ToString()), a => a.Trim(',', ' '))}");
                        // Si hay algún resultado false, será FALSE
                        res = me.Value 
                            ? !result.Any(r => r.HasValue && !r.Value)
                            : !result.Any(r => !r.HasValue || !r.Value);
                        //return result.Any(r => r.HasValue && r.Value) // Si algun resultado tiene valor y es true, TRUE
                        //    || result.All(r => !r.HasValue || r.Value); // Si todos los resultados o son true o no tienen valor (no hay ningún false), TRUE
                    }
                }
                Printer.WriteLine($"RESULT: {res}");
                return res;
            }
        }
        public static string ToOneLineString(this IPermission me)
            => $"{me.Value} - {me.Function.Name} - {me.Scopes.Count()}";
        public static void Print(this IEnumerable<IPermission> me, PrintMode mode)
        {
            switch (mode)
            {
                case PrintMode.OneLine:
                    foreach (var per in me)
                    {
                        Printer.WriteLine(per.Function.Name.PadRight(8, ' ') + " , v:" +
                            per.Value + "".PadRight(per.Value ? 1 : 0, ' ') + " , ss:[" +
                            per.Scopes.Aggregate("", (str, actual) => str + actual + ",", str => str.Trim(',')) +
                            "]");
                    }
                    break;
                case PrintMode.PropertyList:
                    break;
                case PrintMode.Table:
                    var valueLength = me.Select(p => p.Value.ToString().Length).Union(new[] { "VALUE".Length }).Max();
                    var functionLength = me.Select(p => p.Function.Name.ToString().Length).Union(new[] { "FUNCTION".Length }).Max();
                    var typeLength = new[] { "TYPE".Length }.Concat(me.SelectMany(p => p.Scopes.Select(s => s.Discriminator.TypeName.Length))).Max();
                    var nameLength = new[] { "NAME".Length }.Concat(me.SelectMany(p => p.Scopes.Select(s => s.Discriminator.Name.Length))).Max();
                    var propagationLength = new[] { "PROPAGATION".Length }.Concat(me.SelectMany(p => p.Scopes.Select(s => s.Propagation.ToString().Length))).Max();

                    Printer.WriteLine("┌" + ("".PadRight(valueLength, '─')) + "┬" + ("".PadRight(functionLength, '─')) + "┬" + ("".PadRight(typeLength, '─')) + "┬" + ("".PadRight(nameLength, '─')) + "┬" + ("".PadRight(propagationLength, '─')) + "┐");
                    if (me.Any())
                    {
                        Printer.WriteLine("│" + ("VALUE".PadRight(valueLength, ' ')) + "│" + ("FUNCTION".PadRight(functionLength, ' ')) + "│" + ("TYPE".PadRight(typeLength, ' ')) + "│" + ("NAME".PadRight(nameLength, ' ')) + "│" + ("PROPAGATION".PadRight(propagationLength, ' ')) + "│");
                        Printer.WriteLine("├" + ("".PadRight(valueLength, '─')) + "┼" + ("".PadRight(functionLength, '─')) + "┼" + ("".PadRight(typeLength, '─')) + "┼" + ("".PadRight(nameLength, '─')) + "┼" + ("".PadRight(propagationLength, '─')) + "┤");
                    }

                    foreach(var per in me)
                    {
                        var list = per.Scopes.ToList();
                        if (list.Count == 0)
                        {
                            Printer.WriteLine("│" +
                                    per.Value.ToString().PadRight(valueLength, ' ') + "│" +
                                    per.Function.Name.PadRight(functionLength, ' ') + "│" +
                                    ("".PadRight(typeLength, ' ')) + "│" +
                                    ("".PadRight(nameLength, ' ')) + "│" +
                                    ("".PadRight(propagationLength, ' ')) + "│");
                        }
                        else {
                            for (int i = 0; i < list.Count; i++)
                            {
                                Printer.WriteLine("│" +
                                    ((i == 0 ? per.Value.ToString() : "").PadRight(valueLength, ' ')) + "│" +
                                    ((i == 0 ? per.Function.Name : "").PadRight(functionLength, ' ')) + "│" +
                                    (list[i].Discriminator.TypeName.PadRight(typeLength, ' ')) + "│" +
                                    (list[i].Discriminator.Name.PadRight(nameLength, ' ')) + "│" +
                                    (list[i].Propagation.ToString().PadRight(propagationLength, ' ')) + "│");
                            }
                        }
                    }
                    Printer.WriteLine("└" + ("".PadRight(valueLength, '─')) + "┴" + ("".PadRight(functionLength, '─')) + "┴" + ("".PadRight(typeLength, '─')) + "┴" + ("".PadRight(nameLength, '─')) + "┴" + ("".PadRight(propagationLength, '─')) + "┘");
                    break;
            }
        }
    }
    public class PermissionEqualityComparer : IEqualityComparer<IPermission>
    {
        FunctionEqualityComparer funCom = new FunctionEqualityComparer();
        ScopeEqualityComparer scoCom = new ScopeEqualityComparer();
        public bool Equals(IPermission x, IPermission y)
        {
            return AreEquals(x, y);
        }

        public int GetHashCode(IPermission obj)
        {
            if (obj == null) return 0;
            return funCom.GetHashCode(obj.Function) ^ obj.Scopes.Select(s => scoCom.GetHashCode(s)).Aggregate(0, (a, c) => a ^ c) ^ obj.Value.GetHashCode();
        }
        bool AreEquals(object obj1, object obj2)
        {
            // If both are NULL, return TRUE
            if (Equals(obj1, null) && Equals(obj2, null)) return true;
            // If some of them is null, return FALSE
            if (Equals(obj1, null) || Equals(obj2, null)) return false;
            // If any of them are of other type, return FALSE
            if (!(obj1 is IPermission) || !(obj2 is IPermission)) return false;
            var per1 = (IPermission)obj1;
            var per2 = (IPermission)obj2;
            // Use 'Equals' to compare the ids
            return funCom.Equals(per1.Function, per2.Function) &&
                per1.Scopes.All(s => per2.Scopes.Any(s2 => scoCom.Equals(s, s2))) &&
                per2.Scopes.All(s => per1.Scopes.Any(s2 => scoCom.Equals(s, s2))) &&
                per1.Value == per2.Value;
        }
    }
}
