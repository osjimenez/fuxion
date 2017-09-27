using System;
using System.Collections.Generic;
using System.Linq;
using Fuxion.Factories;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.Extensions;

namespace Fuxion.Identity
{
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
        {
            using (var res = Printer.CallResult<bool>())
                return res.Value = me.Can(Functions.Admin).Anything();
        }
        #endregion
        #region Something
        public static bool Anything(this IRolCan me)
        {
            using (var res = Printer.CallResult<bool>())
            {
                var ime = (IInternalRolCan)me;
                if (ime.Rol == null) return false;
                foreach (var fun in ime.Functions)
                {
                    var permissions = ime.Rol.SearchPermissions(false, fun, TypeDiscriminator.Empty);
                    if (!permissions.Any() || permissions.Any(p => p.Scopes.Any()))
                        return res.Value = false;
                }
                return res.Value = true;
            }
        }
        public static bool Something(this IRolCan me)
        {
            using (var res = Printer.CallResult<bool>())
            {
                var ime = (IInternalRolCan)me;
                if (ime.Rol == null) return res.Value = false;
                foreach (var fun in ime.Functions)
                {
                    var permissions = ime.Rol.SearchPermissions(false, fun, TypeDiscriminator.Empty);
                    if (!permissions.Any(p => p.Value))
                        return res.Value = false;
                }
                return res.Value = true;
            }
        }
        #endregion
        #region Can().By..<IDiscriminator>()
        public static bool ByAll(this IRolCan me, TypeDiscriminator typeDiscriminator, params IDiscriminator[] discriminators)
        {
            using(var res = Printer.CallResult<bool>())
                return res.Value = ((IInternalRolCan)me).CheckDiscriminators(true, typeDiscriminator, discriminators);
        }
        public static bool ByAny(this IRolCan me, TypeDiscriminator typeDiscriminator, params IDiscriminator[] discriminators)
        {
            using (var res = Printer.CallResult<bool>())
                return res.Value = ((IInternalRolCan)me).CheckDiscriminators(false, typeDiscriminator, discriminators);
        }
        #endregion
        #region Can().Type's
        // Only one type
        public static bool Type(this IRolCan me, Type type)
        {
            using (var res = Printer.CallResult<bool>())
                return res.Value = ((IInternalRolCan)me).CheckDiscriminators(true,
                    Factory.Get<TypeDiscriminatorFactory>().FromType(type));
        }
        public static bool Type<T>(this IRolCan me) => me.Type(typeof(T));
        #endregion
        #region Can().Instance's
        // One instance
        public static bool Instance<T>(this IRolCan me, T value)
        {
            using (var res = Printer.CallResult<bool>())
                return res.Value = me.AllInstances(new[] { value });
        }
        // Many instances
        public static bool AllInstances<T>(this IRolCan me, IEnumerable<T> values)
        {
            using (var res = Printer.CallResult<bool>())
                return res.Value = ((IInternalRolCan)me).CheckInstances(true, values);
        }
        public static bool AnyInstances<T>(this IRolCan me, IEnumerable<T> values)
        {
            using (var res = Printer.CallResult<bool>())
                return res.Value = ((IInternalRolCan)me).CheckInstances(false, values);
        }
        // -------------------------- IMPLEMENTATION
        private static bool CheckInstances<T>(this IInternalRolCan me, bool forAll, IEnumerable<T> values)
        {
            using(var res = Printer.CallResult<bool>())
            {
                if (me.Rol == null) return res.Value = false;
                bool CheckInstance(T value)
                   => ByAll(me, Factory.Get<TypeDiscriminatorFactory>().FromType<T>(), typeof(T).GetDiscriminatorsOfDiscriminatedProperties(value).ToArray());
                var r = forAll
                    ? values.Where(value => CheckInstance(value)).Count() == values.Count()
                    : values.Where(value => CheckInstance(value)).Any();
                if (me.ThrowExceptionIfCannot && !r)
                    throw new UnauthorizedAccessException($"The rol '{me.Rol.Name}' cannot '{me.Functions.Aggregate("", (a, c) => a + c.Name + "·", a => a.Trim('·'))}' for the given instances '{values}'");
                return res.Value = r;
            }
        }
        #endregion
        #endregion
    }
    #region Fluent Clases
    public interface IRolCan { }
    internal interface IInternalRolCan : IRolCan
    {
        IRol Rol { get; }
        IFunction[] Functions { get; }
        bool ThrowExceptionIfCannot { get; }
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
        public bool ThrowExceptionIfCannot { get; set; }
    }
    #endregion
}