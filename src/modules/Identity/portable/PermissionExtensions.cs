using System.Collections.Generic;
using System.Linq;
using Fuxion.Identity.Helpers;
using System;

namespace Fuxion.Identity
{
    public static class PermissionExtensions
    {
        public static bool IsValid(this IPermission me) { return me.Function != null && (me.Scopes?.All(s => s.Discriminator != null) ?? true) && me.Scopes?.Select(s => s.Discriminator.TypeId).Distinct().Count() == me.Scopes?.Count(); }
        internal static bool Match(this IPermission me, IFunction function, IDiscriminator targetDiscriminator, params IDiscriminator[] discriminators)
        {
            bool res = false;
            using (Printer.Indent2($"CALL {nameof(Match)}:", '│'))
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
                bool Compute()
                {
                    if (function == null || !me.MatchByFunction(function))
                    {
                        Printer.WriteLine($"Matching failed on check the function");
                        return false;
                    }
                    if (!me.MatchByDiscriminatorsInclusionsAndExclusions(targetDiscriminator, discriminators))
                    {
                        Printer.WriteLine($"Matching failed on check the inclusions/exclusions of discriminator");
                        return false;
                    }
                    return true;
                }
                res = Compute();
            }
            Printer.WriteLine($"● RESULT {nameof(Match)}: {res}");
            return res;
        }
        internal static bool MatchByFunction(this IPermission me, IFunction function)
        {
            bool res = false;
            using (Printer.Indent2($"CALL {nameof(MatchByFunction)}:", '│'))
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
            }
            Printer.WriteLine($"● RESULT {nameof(MatchByFunction)}: {res}");
            return res;
        }
        internal static bool MatchByDiscriminatorsInclusionsAndExclusions(this IPermission me, IDiscriminator targetDiscriminator, params IDiscriminator[] discriminators)
        {
            bool res = false;
            using (Printer.Indent2($"CALL {nameof(MatchByDiscriminatorsInclusionsAndExclusions)}:", '│'))
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
                if (discriminators.Any(d => Comparer.AreEquals(d.TypeId, TypeDiscriminator.TypeDiscriminatorId)))
                    throw new ArgumentException("discriminators cannot contains a TypeDiscriminator");
                if (targetDiscriminator == null)
                    throw new ArgumentException("target discriminator cannot be null");
                if (targetDiscriminator != null && !Comparer.AreEquals(targetDiscriminator.TypeId, TypeDiscriminator.TypeDiscriminatorId))
                    throw new ArgumentException("target discriminator must be a TypeDiscriminator");
                bool Compute() {
                    // Si no hay discriminador de tipo, TRUE
                    if (targetDiscriminator == null)
                    {
                        Printer.WriteLine($"'{nameof(targetDiscriminator)}' is null");
                        //return true;
                    }
                    // Si el permiso no define scopes, TRUE
                    if (!me.Scopes.Any())
                    {
                        Printer.WriteLine("Permission hasn't scopes");
                        return true;
                    }
                    // Compruebo el discriminador objetivo
                    {
                        var scopeOfTypeOfTarget = me.Scopes.FirstOrDefault(s => Comparer.AreEquals(s.Discriminator.TypeId, targetDiscriminator?.TypeId));
                        if(scopeOfTypeOfTarget != null)
                        {
                            Printer.WriteLine($"The target discriminator '{targetDiscriminator}' and permission scope '{scopeOfTypeOfTarget}' have same type '{targetDiscriminator.TypeId}', continue");
                            var scopeDiscriminatorRelatedWithTargetDiscriminator = scopeOfTypeOfTarget?.Discriminator
                                .GetAllRelated(scopeOfTypeOfTarget.Propagation)
                                .FirstOrDefault(rel => Comparer.AreEquals(targetDiscriminator.TypeId, rel.TypeId) && Comparer.AreEquals(targetDiscriminator.Id, rel.Id));
                            if(scopeDiscriminatorRelatedWithTargetDiscriminator != null)
                            {
                                Printer.WriteLine($"The target discriminator '{targetDiscriminator}' is related to permission scope '{scopeOfTypeOfTarget}' on discriminator '{scopeDiscriminatorRelatedWithTargetDiscriminator}', check discriminators");

                            }
                            else if((TypeDiscriminator)targetDiscriminator != TypeDiscriminator.Empty)
                            {
                                Printer.WriteLine($"The target discriminator '{targetDiscriminator}' isn't related to permission scope '{scopeOfTypeOfTarget}', FALSE");
                                return false;
                            }
                        }
                        else
                        {
                            Printer.WriteLine($"The target discriminator '{targetDiscriminator}' hasn't any scope with discriminator of its type");
                            if (discriminators.IsNullOrEmpty())
                            {
                                Printer.WriteLine($"Haven't discriminators, VALUE");
                                return me.Value;
                            }
                            else
                                Printer.WriteLine($"Have some discriminators, check discriminators");
                        }
                    }
                    // Compruebo el resto de discriminadores
                    return discriminators.All(dis => { 
                    //foreach (var dis in discriminators)
                    //{
                        var scopeOfTypeOfDiscriminator = me.Scopes.FirstOrDefault(s => Comparer.AreEquals(s.Discriminator.TypeId, dis.TypeId));
                        var scopeDiscriminatorRelatedWithDiscriminator = scopeOfTypeOfDiscriminator?.Discriminator
                            .GetAllRelated(scopeOfTypeOfDiscriminator.Propagation)
                            .FirstOrDefault(rel => Comparer.AreEquals(dis.TypeId, rel.TypeId) && Comparer.AreEquals(dis.Id, rel.Id));

                        if (scopeOfTypeOfDiscriminator != null)
                        {
                            Printer.WriteLine($"The discriminator '{dis}' and permission scope '{scopeOfTypeOfDiscriminator}' have same type '{dis.TypeId}'");
                            if (scopeDiscriminatorRelatedWithDiscriminator != null)
                            {
                                Printer.WriteLine($"The discriminator '{dis}' is related to permission scope '{scopeOfTypeOfDiscriminator}' on discriminator '{scopeDiscriminatorRelatedWithDiscriminator}'");
                                return true;
                            }
                            else
                            {
                                Printer.WriteLine($"The discriminator '{dis}' isn't related to permission scopes, continue");
                                //return true;
                            }
                        }
                        else
                        {
                            Printer.WriteLine($"The permission hasn't any discriminator of type '{dis}', VALUE or !VALUE");
                            //return me.Value;
                            //return true;
                            //return false;
                            return dis.Id == null ? me.Value : !me.Value;
                        }
                        return false;
                        //}
                    });
                }
                res = Compute();
            }
            Printer.WriteLine($"● RESULT {nameof(MatchByDiscriminatorsInclusionsAndExclusions)}: {res}");
            return res;
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
                    var typeLength = new[] { "TYPE".Length }.Concat(me.SelectMany(p => p.Scopes.Select(s => (s.Discriminator.TypeId + "-" + s.Discriminator.TypeName).Length))).Max();
                    var nameLength = new[] { "ID".Length }.Concat(me.SelectMany(p => p.Scopes.Select(s => ((s.Discriminator.Id?.ToString() ?? "null") + "-" + (s.Discriminator.Name ?? "null")).Length))).Max();
                    var propagationLength = new[] { "PROPAGATION".Length }.Concat(me.SelectMany(p => p.Scopes.Select(s => s.Propagation.ToString().Length))).Max();

                    Printer.WriteLine("┌" + ("".PadRight(valueLength, '─')) + "┬" + ("".PadRight(functionLength, '─')) + "╥" + ("".PadRight(typeLength, '─')) + "┬" + ("".PadRight(nameLength, '─')) + "┬" + ("".PadRight(propagationLength, '─')) + "┐");
                    if (me.Any())
                    {
                        Printer.WriteLine("│" + ("VALUE".PadRight(valueLength, ' ')) + "│" + ("FUNCTION".PadRight(functionLength, ' ')) + "║" + ("TYPE".PadRight(typeLength, ' ')) + "│" + ("ID".PadRight(nameLength, ' ')) + "│" + ("PROPAGATION".PadRight(propagationLength, ' ')) + "│");
                        Printer.WriteLine("├" + ("".PadRight(valueLength, '─')) + "┼" + ("".PadRight(functionLength, '─')) + "╫" + ("".PadRight(typeLength, '─')) + "┼" + ("".PadRight(nameLength, '─')) + "┼" + ("".PadRight(propagationLength, '─')) + "┤");
                    }

                    foreach(var per in me)
                    {
                        var list = per.Scopes.ToList();
                        if (list.Count == 0)
                        {
                            Printer.WriteLine("│" +
                                    per.Value.ToString().PadRight(valueLength, ' ') + "│" +
                                    per.Function.Name.PadRight(functionLength, ' ') + "║" +
                                    ("".PadRight(typeLength, ' ')) + "│" +
                                    ("".PadRight(nameLength, ' ')) + "│" +
                                    ("".PadRight(propagationLength, ' ')) + "│");
                        }
                        else {
                            for (int i = 0; i < list.Count; i++)
                            {
                                Printer.WriteLine("│" +
                                    ((i == 0 ? per.Value.ToString() : "").PadRight(valueLength, ' ')) + "│" +
                                    ((i == 0 ? per.Function.Name : "").PadRight(functionLength, ' ')) + "║" +
                                    ((list[i].Discriminator.TypeId + "-" + list[i].Discriminator.TypeName).PadRight(typeLength, ' ')) + "│" +
                                    (((list[i].Discriminator.Id?.ToString() ?? "null") + "-" + (list[i].Discriminator.Name ?? "null")).PadRight(nameLength, ' ')) + "│" +
                                    (list[i].Propagation.ToString().PadRight(propagationLength, ' ')) + "│");
                            }
                        }
                    }
                    Printer.WriteLine("└" + ("".PadRight(valueLength, '─')) + "┴" + ("".PadRight(functionLength, '─')) + "╨" + ("".PadRight(typeLength, '─')) + "┴" + ("".PadRight(nameLength, '─')) + "┴" + ("".PadRight(propagationLength, '─')) + "┘");
                    break;
            }
        }
    }
}
