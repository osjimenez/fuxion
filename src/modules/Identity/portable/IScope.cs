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
    public static class ScopeExtensions
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
                    var type = me.Max(s => s.Discriminator.TypeName.Length);
                    var name = me.Max(s => s.Discriminator.Name.Length);
                    var pro = me.Max(s => s.Propagation.ToString().Length);
                    Printer.Print("┌" + ("".PadRight(type, '─')) + "┬" + ("".PadRight(name, '─')) + "┬" + ("".PadRight(pro, '─')) + "┐");
                    Printer.Print("│" + "TYPE".PadRight(type, ' ') + "│" + "NAME".PadRight(name, ' ') + "│" + "PROPAGATION".PadRight(pro, ' ') + "│");
                    Printer.Print("├" + ("".PadRight(type, '─')) + "┼" + ("".PadRight(name, '─')) + "┼" + ("".PadRight(pro, '─')) + "┤");
                    foreach (var sco in me) Printer.Print("│" + sco.Discriminator.TypeName.PadRight(type, ' ') + "│" + sco.Discriminator.Name.PadRight(name, ' ') + "│" + sco.Propagation.ToString().PadRight(pro, ' ') + "│");
                    Printer.Print("└" + ("".PadRight(type, '─')) + "┴" + ("".PadRight(name, '─')) + "┴" + ("".PadRight(pro, '─')) + "┘");
                    break;
            }
        }
    }
    public enum PrintMode
    {
        OneLine,
        PropertyList,
        Table
    }
}
