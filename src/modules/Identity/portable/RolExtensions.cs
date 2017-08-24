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
        #endregion
        #region Something
        public static bool Anything(this IRolCan me)
        {
            var ime = (IInternalRolCan)me;
            if (ime.Rol == null) return false;
            foreach (var fun in ime.Functions)
            {
                var permissions = ime.Rol.SearchPermissions(fun, TypeDiscriminator.Empty);
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
                var permissions = ime.Rol.SearchPermissions(fun, TypeDiscriminator.Empty);
                if (!permissions.Any(p => p.Value))
                    return false;
            }
            return true;
        }
        #endregion
        #region Can().By..<IDiscriminator>()
        public static bool ByAll(this IRolCan me, TypeDiscriminator typeDiscriminator, params IDiscriminator[] discriminators)
        {
            bool res = false;
            using (Printer.Indent2($"CALL {nameof(ByAll)}:", '│'))
            {
                res = ((IInternalRolCan)me).CheckDiscriminators(true, typeDiscriminator, discriminators);
            }
            Printer.WriteLine($"● RESULT {nameof(ByAll)}: {res}");
            return res;
        }
        public static bool ByAny(this IRolCan me, TypeDiscriminator typeDiscriminator, params IDiscriminator[] discriminators)
            => ((IInternalRolCan)me).CheckDiscriminators(false, typeDiscriminator, discriminators);
        #endregion
        #region Can().Type's
        // Only one type
        public static bool Type(this IRolCan me, Type type) {
            bool res = false;
            using (Printer.Indent2($"CALL {nameof(Type)}:", '│'))
            {
                res = ((IInternalRolCan)me).CheckDiscriminators(true,
                    Factory.Get<TypeDiscriminatorFactory>().FromType(type)
                    //,type.GetDiscriminatorsOfDiscriminatedProperties().ToArray()
                    );
            }
            Printer.WriteLine($"● RESULT {nameof(Type)}: {res}");
            return res;
        }
        public static bool Type<T>(this IRolCan me) => me.Type(typeof(T));
        
        // Two types
        //public static bool AllTypes<T1, T2>(this IRolCan me) => me.AllTypes(typeof(T1), typeof(T2));
        //public static bool AnyType<T1, T2>(this IRolCan me) => me.AnyType(typeof(T1), typeof(T2));
        //// Three types
        //public static bool AllTypes<T1, T2, T3>(this IRolCan me) => me.AllTypes(typeof(T1), typeof(T2), typeof(T3));
        //public static bool AnyType<T1, T2, T3>(this IRolCan me) => me.AnyType(typeof(T1), typeof(T2), typeof(T3));
        //// Many types
        //public static bool AllTypes(this IRolCan me, params Type[] types) => me.ByAll(types.Select(t => Factory.Get<TypeDiscriminatorFactory>().FromType(t)).RemoveNulls().ToArray());




        //public static bool AllTypes2(this IRolCan me, params Type[] types)
        //{
        //    bool res = false;
        //    using (Printer.Indent2($"CALL {nameof(AllTypes2)}:", '│'))
        //    {
        //        res = me.ByAll2(types.Select(t => Factory.Get<TypeDiscriminatorFactory>().FromType(t)).RemoveNulls().ToArray());
        //    }
        //    Printer.WriteLine($"● RESULT {nameof(AllTypes2)}: {res}");
        //    return res;
        //}



        //public static bool AnyType(this IRolCan me, params Type[] types) => me.ByAny(types.Select(t => Factory.Get<TypeDiscriminatorFactory>().FromType(t)).RemoveNulls().ToArray());
        #endregion
        #region Can().Instance's
        // One instance
        public static bool Instance<T>(this IRolCan me, T value)
        {
            var res = false;
            using (Printer.Indent2($"CALL {nameof(Instance)}:", '│'))
            {
                res = me.AllInstances(new[] { value });
            }
            Printer.WriteLine($"● RESULT {nameof(Instance)}: {res}");
            return res;
        }
        // Many instances
        public static bool AllInstances<T>(this IRolCan me, IEnumerable<T> values)
        {
            var res = false;
            using (Printer.Indent2($"CALL {nameof(AllInstances)}:", '│'))
            {
                res = ((IInternalRolCan)me).CheckInstances(true, values);
            }
            Printer.WriteLine($"● RESULT {nameof(AllInstances)}: {res}");
            return res;
        }
        public static bool AnyInstances<T>(this IRolCan me, IEnumerable<T> values)
        {
            var res = false;
            using (Printer.Indent2($"CALL {nameof(AnyInstances)}:", '│'))
            {
                res = ((IInternalRolCan)me).CheckInstances(false, values);
            }
            Printer.WriteLine($"● RESULT {nameof(AnyInstances)}: {res}");
            return res;
        }
        // -------------------------- IMPLEMENTATION
        private static bool CheckInstances<T>(this IInternalRolCan me, bool forAll, IEnumerable<T> values)
        {
            bool res = false;
            using (Printer.Indent2($"CALL {nameof(CheckInstances)}:", '│'))
            {
                bool Compute()
                {
                    if (me.Rol == null) return false;
                    bool CheckInstance(T value)
                       => ByAll(me, Factory.Get<TypeDiscriminatorFactory>().FromType<T>(), typeof(T).GetDiscriminatorsOfDiscriminatedProperties(value).ToArray());
                    var r = forAll
                        ? values.Where(value => CheckInstance(value)).Count() == values.Count()
                        : values.Where(value => CheckInstance(value)).Any();
                    if (me.ThrowExceptionIfCannot && !r)
                        throw new UnauthorizedAccessException($"The rol '{me.Rol.Name}' cannot '{me.Functions.Aggregate("", (a, c) => a + c.Name + "·", a => a.Trim('·'))}' for the given instances '{values}'");
                    return r;
                }
                res = Compute();
            }
            Printer.WriteLine($"● RESULT {nameof(CheckInstances)}: {res}");
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