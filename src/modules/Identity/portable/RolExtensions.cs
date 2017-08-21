using System;
using System.Collections.Generic;
using System.Linq;
using Fuxion.Factories;
using System.Reflection;

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
            => me.Can(Functions.Admin).Anything();
        public static bool IsRoot2(this IRol me)
            => me.Can(Functions.Admin).Anything2();
        #endregion
        #region Something
        public static bool Anything(this IRolCan me)
        {
            var ime = (IInternalRolCan)me;
            if (ime.Rol == null) return false;
            foreach (var fun in ime.Functions)
            {
                var permissions = ime.Rol.SearchPermissions(fun);
                if (!permissions.Any() || permissions.Any(p => p.Scopes.Any()))
                    return false;
            }
            return true;
        }
        public static bool Anything2(this IRolCan me)
        {
            var ime = (IInternalRolCan)me;
            if (ime.Rol == null) return false;
            foreach (var fun in ime.Functions)
            {
                var permissions = ime.Rol.SearchPermissions2(fun, null);
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
                var permissions = ime.Rol.SearchPermissions(fun);
                if (!permissions.Any(p => p.Value))
                    return false;
            }
            return true;
        }
        public static bool Something2(this IRolCan me)
        {
            var ime = (IInternalRolCan)me;
            if (ime.Rol == null) return false;
            foreach (var fun in ime.Functions)
            {
                var permissions = ime.Rol.SearchPermissions2(fun, null);
                if (!permissions.Any(p => p.Value))
                    return false;
            }
            return true;
        }
        #endregion
        #region Can().By..<IDiscriminator>()
        public static bool ByAll(this IRolCan me, params IDiscriminator[] discriminators)
            => ((IInternalRolCan)me).CheckDiscriminators(true, discriminators);
        public static bool ByAll2(this IRolCan me, params IDiscriminator[] discriminators)
            => ((IInternalRolCan)me).CheckDiscriminators2(true, discriminators.First(), discriminators.Skip(1).ToArray());
        public static bool ByAny(this IRolCan me, params IDiscriminator[] discriminators)
            => ((IInternalRolCan)me).CheckDiscriminators(false, discriminators);
        #endregion
        #region Can().Type's
        // Only one type
        public static bool Type(this IRolCan me, Type type) => me.AllTypes(type);
        public static bool Type<T>(this IRolCan me) => me.AllTypes(typeof(T));
        public static bool Type2<T>(this IRolCan me) => ((IInternalRolCan)me).CheckDiscriminators2(true,
            Factory.Get<TypeDiscriminatorFactory>().FromType<T>(),
            typeof(T).GetDiscriminatorsOfDiscriminatedProperties().ToArray());
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
        public static bool Instance2<T>(this IRolCan me, T value) => me.AllInstances2(new[] { value });
        //public static bool Instance2<T>(this IRolCan me, T value)
        //    => ByAll2(me, new IDiscriminator[]
        //    {
        //        Factory.Get<TypeDiscriminatorFactory>().FromType<T>(),
        //    }.Concat(typeof(T).GetDiscriminatedProperties().Select(p =>
        //    {
        //        var val = p.PropertyInfo.GetValue(value);
        //        if (val == null) return Discriminator.Empty(p.DiscriminatorType);
        //        return Discriminator.ForId(p.DiscriminatorType, val);
        //    }).RemoveNulls())
        //    .ToArray());
        // Many instances
        public static bool AllInstances<T>(this IRolCan me, IEnumerable<T> values) => ((IInternalRolCan)me).CheckInstances(true, values);
        public static bool AllInstances2<T>(this IRolCan me, IEnumerable<T> values) => ((IInternalRolCan)me).CheckInstances2(true, values);
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
        private static bool CheckInstances2<T>(this IInternalRolCan me, bool forAll, IEnumerable<T> values)
        {
            if (me.Rol == null) return false;
            bool CheckInstance(T value)
               => ByAll2(me, new IDiscriminator[]
                {
                    Factory.Get<TypeDiscriminatorFactory>().FromType<T>(),
                }.Concat(typeof(T).GetDiscriminatorsOfDiscriminatedProperties(value))
                .ToArray());
            var res = forAll
                ? values.Where(value => CheckInstance(value)).Count() == values.Count()
                : values.Where(value => CheckInstance(value)).Any();
            if (me.ThrowExceptionIfCannot && !res)
                throw new UnauthorizedAccessException($"The rol '{me.Rol.Name}' cannot '{me.Functions.Aggregate("", (a, c) => a + c.Name + "·", a => a.Trim('·'))}' for the given instances '{values}'");
            return res;
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