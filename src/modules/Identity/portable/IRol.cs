using System;
using System.Collections.Generic;
using Fuxion.Identity.Helpers;
using System.Linq;
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
        //public static bool IsValid(this IRol me) { return me.Id != Guid.Empty && !string.IsNullOrWhiteSpace(me.Name); }
        //public static bool IsValid(this IRol me) { return !Comparer.AreEquals(me.Id, me.Id?.GetType().GetDefaultValue()) && !string.IsNullOrWhiteSpace(me.Name); }
        //public static bool IsValid(this IRol me) { return !string.IsNullOrWhiteSpace(me.Name); }
        internal static bool IsFunctionAssigned(this IRol me, IFunction function, IDiscriminator[] discriminators, Action<string, bool> console, out IPermission deniedPermissionMatched)
        {
            Action<string, bool> con = (m, i) => { if (console != null) console(m, i); };
            con($"Comprobando asignación de funciones\r\n   Rol: {me.Name}\r\n   Funcion: {function.Name}\r\n   Discriminadores:\r\n{discriminators.Aggregate("", (a, c) => $"{a}      {c.Name}\r\n")}", false);
            // Parameters validation
            if (!function.IsValid()) throw new InvalidStateException($"The '{nameof(function)}' parameter has an invalid state");
            if (discriminators == null || !discriminators.Any()) throw new ArgumentException($"The '{nameof(discriminators)}' pararameter cannot be null or empty", nameof(discriminators));
            var invalidDiscriminator = discriminators.FirstOrDefault(d => !d.IsValid());
            if (invalidDiscriminator != null) throw new InvalidStateException($"The '{invalidDiscriminator}' discriminator has an invalid state");

            Func<IRol, IEnumerable<IPermission>> getPers = null;
            getPers = new Func<IRol, IEnumerable<IPermission>>(rol =>
            {
                var res = new List<IPermission>();
                if (rol.Permissions != null) res.AddRange(rol.Permissions);
                if(rol.Groups != null) foreach (var gro in rol.Groups) res.AddRange(getPers(gro));
                return res;
            });
            var permissions = getPers(me);


            // First, must take only permissions that match with the function and discriminators
            if(permissions == null || !permissions.Any())
            {
                con($"Resultado: NO VALIDO - El rol no tiene permisos definidos", true);
                deniedPermissionMatched = null;
                return false;
            }
            var matchs = permissions.Where(p => p.Match(function, discriminators, con)).ToList();
            con($"   Match => {matchs.Count()}", true);
            // Now, let's go to check that does not match any denegation permission
            var den = matchs.FirstOrDefault(p => !p.Value);
            if (den != null)
            {
                deniedPermissionMatched = den;
                con($"Resultado: NO VALIDO - Denegado por un permiso", true);
                return false;
            }
            deniedPermissionMatched = null;
            // Now, let's go to check that match at least one grant permission
            if (!matchs.Any(p => p.Value))
            {
                con($"Resultado: NO VALIDO - Denegado por no encontrar un permiso de concesión", true);
                return false;
            }
            con($"Resultado: VALIDO", true);
            return true;
        }
        internal static void CheckFunctionAssigned(this IRol me, IFunction function, IDiscriminator[] discriminators, Action<string,bool> console)
        {
            IPermission den;
            me.IsFunctionAssigned(function, discriminators, console, out den);
            if (den != null)
                throw new UnauthorizedAccessException($"The function {function.Name} was requested for these discriminators:"
                                                      + discriminators.Aggregate("", (a, n) => $"{a}\n - {n.TypeName} = {n.Name}")
                                                      + "\nAnd have been matched in:\n" + den);
            else
                throw new UnauthorizedAccessException($"The function {function.Name} was requested for these discriminators:"
                                                          + discriminators.Aggregate("", (a, n) => $"{a}\r\n - {n.TypeName} = {n.Name}")
                                                          + "\nAnd don't match any permission");
        }
        public static IRolCan Can(this IRol me, params IFunction[] functions) { return new RolCan(me); }
        public static IRolFilter<T> Filter<T>(this IRol me, IQueryable<T> source) { return new RolFilter<T>(me); }


        public static bool In<T>(this IRolCan me) { return false; }
        public static bool This<T>(this IRolCan me, T value) { return false; }
        public static bool Any<T>(this IRolCan me, IEnumerable<T> value) { return false; }
        public static bool All<T>(this IRolCan me, IEnumerable<T> value) { return false; }

        public static IQueryable<T> For<T>(this IRolFilter<T> me, IFunction function) { return null; }
        public static IQueryable<T> ForAny<T>(this IRolFilter<T> me, IEnumerable<IFunction> functions) { return null; }
        public static IQueryable<T> ForAny<T>(this IRolFilter<T> me, params IFunction[] functions) { return null; }
        public static IQueryable<T> ForAll<T>(this IRolFilter<T> me, IEnumerable<IFunction> functions) { return null; }
        public static IQueryable<T> ForAll<T>(this IRolFilter<T> me, params IFunction[] functions) { return null; }

        //public static IQueryable<T> WhereCan<T>(this IQueryable<T> me, IFunction function) { return null; }
        public static IQueryable<T> WhereCan<T>(this IQueryable<T> me, params IFunction[] functions) { return null; }
        public static IQueryable<T> WhereCanAny<T>(this IQueryable<T> me, params IFunction[] functions) { return null; }
    }
    public interface IRolCan { }
    class RolCan : IRolCan
    {
        public RolCan(IRol rol) { Rol = rol; }
        public IRol Rol { get; set; }
    }
    public interface IRolFilter<T> { }
    class RolFilter<T> : IRolFilter<T>
    {
        public RolFilter(IRol rol) { Rol = rol; }
        public IRol Rol { get; set; }
    }
}
