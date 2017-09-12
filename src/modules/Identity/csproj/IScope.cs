using static Fuxion.Identity.Helpers.Comparer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fuxion.Identity
{
    public interface IScope {
        IDiscriminator Discriminator { get; }
        ScopePropagation Propagation { get; }
    }
    public static class IScope_Extensions
    {
        public static string ToOneLineString(this IScope me)
        {
            return $"{me.Discriminator} - {me.Propagation}";
        }
        public static bool IsValid(this IScope me) { return me.Discriminator != null && me.Discriminator.IsValid(); }
        public static IEnumerable<IDiscriminator> AllDiscriminators(this IScope me)
        {
            var res = new List<IDiscriminator>();
            if (me.Propagation.HasFlag(ScopePropagation.ToExclusions))
                res.AddRange(me.Discriminator.GetAllExclusions());
            if (me.Propagation.HasFlag(ScopePropagation.ToMe))
                res.Add(me.Discriminator);
            if (me.Propagation.HasFlag(ScopePropagation.ToInclusions))
                res.AddRange(me.Discriminator.GetAllInclusions());
            return res.Distinct();
        }
        public static void Print(this IEnumerable<IScope> me, PrintMode mode)
        {
            switch (mode)
            {
                case PrintMode.OneLine:
                    break;
                case PrintMode.PropertyList:
                    break;
                case PrintMode.Table:
                    var typeLength = new[] { "TYPE".Length }.Concat(me.Select(p => p.Discriminator.TypeName.Length)).Max();
                    var nameLength = new[] { "NAME".Length }.Concat(me.Select(p => p.Discriminator.Name.Length)).Max();
                    var propagationLength = new[] { "PROPAGATION".Length }.Concat(me.Select(p => p.Propagation.ToString().Length)).Max();


                    //var type = me.Max(s => s.Discriminator.TypeName.Length);
                    //var name = me.Max(s => s.Discriminator.Name.Length);
                    //var pro = me.Max(s => s.Propagation.ToString().Length);
                    Printer.WriteLine("┌" + ("".PadRight(typeLength, '─')) + "┬" + ("".PadRight(nameLength, '─')) + "┬" + ("".PadRight(propagationLength, '─')) + "┐");
                    Printer.WriteLine("│" + "TYPE".PadRight(typeLength, ' ') + "│" + "NAME".PadRight(nameLength, ' ') + "│" + "PROPAGATION".PadRight(propagationLength, ' ') + "│");
                    Printer.WriteLine("├" + ("".PadRight(typeLength, '─')) + "┼" + ("".PadRight(nameLength, '─')) + "┼" + ("".PadRight(propagationLength, '─')) + "┤");
                    foreach (var sco in me) Printer.WriteLine("│" + sco.Discriminator.TypeName.PadRight(typeLength, ' ') + "│" + sco.Discriminator.Name.PadRight(nameLength, ' ') + "│" + sco.Propagation.ToString().PadRight(propagationLength, ' ') + "│");
                    Printer.WriteLine("└" + ("".PadRight(typeLength, '─')) + "┴" + ("".PadRight(nameLength, '─')) + "┴" + ("".PadRight(propagationLength, '─')) + "┘");
                    break;
            }
        }
    }
    public class ScopeEqualityComparer : IEqualityComparer<IScope>
    {
        DiscriminatorEqualityComparer disCom = new DiscriminatorEqualityComparer();
        public bool Equals(IScope x, IScope y)
        {
            return AreEquals(x, y);
        }

        public int GetHashCode(IScope obj)
        {
            if (obj == null) return 0;
            return disCom.GetHashCode(obj.Discriminator) ^ obj.Propagation.GetHashCode();
        }
        bool AreEquals(object obj1, object obj2)
        {
            // If both are NULL, return TRUE
            if (Equals(obj1, null) && Equals(obj2, null)) return true;
            // If some of them is null, return FALSE
            if (Equals(obj1, null) || Equals(obj2, null)) return false;
            // If any of them are of other type, return FALSE
            if (!(obj1 is IScope) || !(obj2 is IScope)) return false;
            var sco1 = (IScope)obj1;
            var sco2 = (IScope)obj2;
            // Use 'Equals' to compare the ids
            return disCom.Equals(sco1.Discriminator,sco2.Discriminator) &&
                AreEquals(sco1.Propagation, sco2.Propagation);
        }
    }
}
