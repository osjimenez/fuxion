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
using static Fuxion.Identity.IdentityHelper;
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
        public static IRolCan Can(this IRol me, params IFunction[] functions) 
            => new _RolCan(me, functions, false);
        public static IRolCan EnsureCan(this IRol me, params IFunction[] functions) 
            => new _RolCan(me, functions, true);
        public static bool IsRoot(this IRol me) 
            => me.Can(Functions.Admin).Anything();
        #endregion
        #region Something
        public static bool Anything(this IRolCan me)
        {
            var ime = (IInternalRolCan)me;
            if (ime.Rol == null) return false;
            foreach (var fun in ime.Functions)
            {
                var permissions = SearchPermissions(ime.Rol, fun);
                if (!permissions.Any() || permissions.Any(p => p.Scopes.Any()))
                    return false;
            }
            return true;
        }
        public static bool Something(this IRolCan me)
        {
            var ime = (IInternalRolCan)me;
            if (ime.Rol == null) return false;
            foreach (var fun in ime.Functions)
            {
                var permissions = SearchPermissions(ime.Rol, fun);
                if (!permissions.Any(p => p.Value))
                    return false;
            }
            return true;
        }
        #endregion
        #region Can().By..<IDiscriminator>()
        public static bool ByAll(this IRolCan me, params IDiscriminator[] discriminators)
            => CheckDiscriminators((IInternalRolCan)me, true, discriminators);
        public static bool ByAny(this IRolCan me, params IDiscriminator[] discriminators)
            => CheckDiscriminators((IInternalRolCan)me, false, discriminators);
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
