using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    }
}
