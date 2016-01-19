using System;
using System.Collections.Generic;
using Fuxion.Identity.Helpers;
using System.Linq;
namespace Fuxion.Identity
{
    public interface IRol
    {
        object Id { get; }
        string Name { get; }
        IEnumerable<IGroup> Groups { get; }
        IEnumerable<IPermission> Permissions { get; }
    }
    public interface IRol<TId> : IRol
    {
        new TId Id { get; }
    }
    public static class RolExtensions
    {
        //public static bool IsValid(this IRol me) { return me.Id != Guid.Empty && !string.IsNullOrWhiteSpace(me.Name); }
        public static bool IsValid(this IRol me) { return !Comparer.AreEquals(me.Id, me.Id?.GetType().GetDefaultValue()) && !string.IsNullOrWhiteSpace(me.Name); }

        public static void CheckFunctionAssigned(this IRol me, FunctionGraph functions, IFunction function, params IDiscriminator[] discriminators)
        {
            // Parameters validation
            if (!function.IsValid()) throw new InvalidStateException($"The '{nameof(function)}' parameter has an invalid state");
            if (discriminators == null || !discriminators.Any()) throw new ArgumentException($"The '{nameof(discriminators)}' pararameter cannot be null or empty", nameof(discriminators));
            var invalidDiscriminator = discriminators.FirstOrDefault(d => !d.IsValid());
            if (invalidDiscriminator != null) throw new InvalidStateException($"The '{invalidDiscriminator}' discriminator has an invalid state");
            // First, must take only permissions that match with the function and discriminators
            var matchs = me.Permissions.Where(p => p.Match(functions, function, discriminators));
            // Now, let's go to check that does not match any denegation permission
            var den = matchs.FirstOrDefault(p => !p.Value);
            if (den != null)
                throw new UnauthorizedAccessException($"The function {function.Name} was requested for these discriminators:"
                                                      + discriminators.Aggregate("", (a, n) => $"{a}\n - {n.TypeName} = {n.Name}")
                                                      + "\nAnd have been matched in:\n" + den);
            // Now, let's go to check that match at least one grant permission
            if (!matchs.Any(p => p.Value))
                throw new UnauthorizedAccessException($"The function {function.Name} was requested for these discriminators:"
                                                      + discriminators.Aggregate("", (a, n) => $"{a}\n - {n.TypeName} = {n.Name}")
                                                      + "\nAnd don't match any permission");
        }
    }
}
