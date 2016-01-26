using System;
using System.Collections.Generic;
using System.Linq;
using Fuxion.Identity.Helpers;
namespace Fuxion.Identity
{
    public interface IIdentity : IRol//, IAggregate
    {
        //IEnumerable<IPermission> Permissions { get; }
        string UserName { get; }
        byte[] PasswordHash { get; }
        byte[] PasswordSalt { get; }
    }
    public static class IdentityExtensions
    {
        public static string GetCache(this IIdentity me)
        {
            
            return null;
        }
        //    public static void CheckFunctionAssigned(this IIdentity me, FunctionCollection functions, IFunction function, params IDiscriminator[] discriminators) {
        //        // Parameters validation
        //        if (!function.IsValid()) throw new InvalidStateException($"The '{nameof(function)}' parameter has an invalid state");
        //        if (discriminators == null || !discriminators.Any()) throw new ArgumentException($"The '{nameof(discriminators)}' pararameter cannot be null or empty", nameof(discriminators));
        //        var invalidDiscriminator = discriminators.FirstOrDefault(d => !d.IsValid());
        //        if (invalidDiscriminator != null) throw new InvalidStateException($"The '{invalidDiscriminator}' discriminator has an invalid state");
        //        // First, must take only permissions that match with the function and discriminators
        //        var matchs = me.Permissions.Where(p => p.Match(functions, function, discriminators));
        //        // Now, let's go to check that does not match any denegation permission
        //        var den = matchs.FirstOrDefault(p => !p.Value);
        //        if (den != null)
        //            throw new UnauthorizedAccessException($"The function {function.Name} was requested for these discriminators:"
        //                                                  + discriminators.Aggregate("", (a, n) => $"{a}\n - {n.TypeName} = {n.Name}")
        //                                                  + "\nAnd have been matched in:\n" + den);
        //        // Now, let's go to check that match at least one grant permission
        //        if (!matchs.Any(p => p.Value))
        //            throw new UnauthorizedAccessException($"The function {function.Name} was requested for these discriminators:"
        //                                                  + discriminators.Aggregate("", (a, n) => $"{a}\n - {n.TypeName} = {n.Name}")
        //                                                  + "\nAnd don't match any permission");
        //    }
    }
    //class Identity : Rol, IIdentity
    //{
    //    //public Identity(string accessToken) { }
    //    public Identity(Guid id, string name, IGroup[] groups, params IPermission[] permissions) : base(id, name, groups)
    //    {
    //        Permissions = permissions;
    //        Functions = new FunctionCollection();
    //    }
    //    public IEnumerable<IPermission> Permissions { get; private set; }
    //    public FunctionCollection Functions { get; private set; }
    //    // NOTE: we validate incoming data (this is filled from an event coming 
    //    // from the registration BC) so that when EF saves it will fail if it's invalid.
    //    //[RegularExpression(@"[\w-]+(\.?[\w-])*\@[\w-]+(\.[\w-]+)+", ErrorMessage = "Email was incorrect.")]
    //    //public string Email { get; set; }
    //    public void CheckFunctionAssigned(IFunction function, params IDiscriminator[] discriminators)
    //    {
    //        // Validacion de parámetros
    //        if (!function.IsValid()) throw new InvalidStateException($"The '{nameof(function)}' parameter has an invalid state");
    //        if (discriminators == null || !discriminators.Any()) throw new ArgumentException($"The '{nameof(discriminators)}' pararameter cannot be null or empty", nameof(discriminators));
    //        var invalidDiscriminator = discriminators.FirstOrDefault(d => !d.IsValid());
    //        if (invalidDiscriminator != null) throw new InvalidStateException($"The '{invalidDiscriminator}' discriminator has an invalid state");

    //        // Primero vamos a coger solo los permisos que encajan con la función y los discriminadores
    //        var matchs = Permissions.Where(p => p.Match(Functions, function, discriminators));
    //        // Ahora, vamos a comprobar que no ha encajado en ningún permiso de denegación
    //        var den = matchs.FirstOrDefault(p => !p.Value);
    //        if (den != null)
    //            throw new UnauthorizedAccessException($"Me han solicitado la función '{function.Name}' en los siguientes discriminadores:"
    //                                                  + discriminators.Aggregate("", (a, n) => $"{a}\n - {n.TypeName} = {n.Name}")
    //                                                  + "\nY ha coincidido en:\n" + den);
    //        // Ahora, vamos a comprobar que ha encajado al menos en un permiso de concesión
    //        if (!matchs.Any(p => p.Value))
    //            throw new UnauthorizedAccessException($"Me han solicitado la función '{function.Name}' en los siguientes discriminadores:"
    //                                                  + discriminators.Aggregate("", (a, n) => $"{a}\n - {n.TypeName} = {n.Name}")
    //                                                  + "\nY no ha coincidido ningún permiso");



    //        // trás la reducción, si quedan permisos, querrá decir que esos permisos no han sido
    //        // descartados en la reducción y por tanto esta identidad 

    //        // Funciones: Con esto me quito los permisos de funciones que 
    //        //var res = ReducePermissionsByFunction(Permissions, function);

    //        // Si alguno de los permisos no contiene ninguno de los discriminaodres, devuelvo true
    //        //res = ReduceByDiscriminatorType(res, discriminators);

    //        //// Filtro por discriminator path
    //        //res = res.Where(p => discriminators.All(d => p.Scopes.Any(s =>
    //        //    (s.PropagateToMe && d.Value == s.DiscriminatorPath) ||
    //        //    (s.PropagateToParents && s.DiscriminatorPath.StartsWith(d.Value)) ||
    //        //    (s.PropagateToChilds && d.Value == null || d.Value.StartsWith(s.DiscriminatorPath))
    //        //    ))).ToList();
    //        ////            res = res.Where(p => p.Scopes.All(s =>
    //        ////                (!s.PropagateToMe || discriminators.Select(pr => pr.Value).Contains(s.DiscriminatorPath)) &&
    //        ////                (!s.PropagateToParents || discriminators.Any(pr => s.DiscriminatorPath.StartsWith(pr.Value))) &&
    //        ////                (!s.PropagateToChilds || discriminators.Any(pr => pr.Value == null || pr.Value.StartsWith(s.DiscriminatorPath)))));
    //        //if (!res.Any())
    //        //    throw new UnauthorizedAccessException("FAIL");

    //        //throw new UnauthorizedAccessException("Identity '" + Name +
    //        //                                      "' have not assigned function '" + function + "' for '" +
    //        //                                      discriminators.Aggregate("", (c, p) => c + p.Id + ":" + p..Value.Split('/').Last() + " AND ", s => s.TrimEnd(' ', 'A', 'N', 'D')) +
    //        //                                      "' discriminator" + (discriminators.Count() < 2 ? "" : "s"));


    //    }
    //    /// <summary>
    //    /// Reduce una lista de permisos omitiendo los que no sean relevantes según el tipo de unos discriminadores dados.
    //    /// </summary>
    //    /// <param name="permissions"></param>
    //    /// <param name="discriminators"></param>
    //    /// <returns></returns>
    //    //private static IEnumerable<Permission> ReduceByDiscriminatorType(IEnumerable<Permission> permissions, IEnumerable<Discriminator> discriminators)
    //    //{
    //    //    // Quito los permisos 
    //    //    return permissions.Where(p => p.Scopes.All(s => !discriminators.Select(d => d.TypeId).Contains(s.Discriminator.TypeId)));
    //    //}
    //    //private static bool ReduceByDiscriminatorType(IEnumerable<Permission> res, IEnumerable<Discriminator> discriminators)
    //    //{
    //    //    // Si alguno de los permisos no contiene ninguno de los discriminaodres, devuelvo true
    //    //    return !res.Any(p => p.Scopes.All(s => !discriminators.Select(d => d.TypeId).Contains(s.Discriminator.TypeId)));
    //    //}


    //    /// <summary>
    //    /// Reduce una lista de permisos omitiendo los que no sean relevantes según la función dada.
    //    /// </summary>
    //    /// <param name="permissions"></param>
    //    /// <param name="function"></param>
    //    /// <returns></returns>
    //    //private IEnumerable<Permission> ReducePermissionsByFunction(IEnumerable<Permission> permissions, Function function)
    //    //{
    //    //    permissions = permissions.Where(p =>
    //    //        // Tiene la misma funcion. 
    //    //        // Ejemplo: Hay que incluir este permiso porque la funcion es relevante
    //    //        p.Function == function ||

    //    //        // Es un permiso de denegacion y la funcion esta incluida.
    //    //        // Ejemplo: Me deniegan EDIT y la funcion a comprobar es de READ
    //    //        // Nota: Me da igual el resto de elementos de este permiso, lo debo incluir. Si me han denegado la edición de 'algo'
    //    //        //       y yo ahora estoy comprobando si puedo leer, sea lo que sea que no me dejan editar, no significa que no lo pueda leer.
    //    //        (!p.Value && Functions.GetGraph().GetDescendants(function).Contains(p.Function)) ||

    //    //        // Es un permios de concesion y la funcion del permiso esta excluida por la función a comprobar

    //    //        // Ejemplo de inclusión: Me conceden EDIT y la funcion a comprobar es de READ
    //    //        // Nota: Me da igual el resto de elementos de este permiso, lo debo incluir. Si me han concedido la edición de 'algo'
    //    //        //       y yo ahora estoy comprobando si puedo leerlo, sea lo que sea que me dejen editar, si lo puedo editar lo podré leer,
    //    //        //       por tanto, este permiso aún me puede servir para verificar si puedo leerlo.

    //    //        // Ejemplo de exclusión: Me conceden READ y la funcion a comprobar es de EDIT
    //    //        // Nota2: Me da igual el resto de elementos de este permiso, lo debo excluir. Si me han concedido la lectura de 'algo'
    //    //        //       y yo ahora estoy comprobando si puedo editarlo, sea lo que sea que me dejen leer este permiso nunca me servirá 
    //    //        //       para verificar si me dejan editarlo.
    //    //        (p.Value && Functions.GetGraph().GetAscendants(function).Contains(p.Function))).ToList();
    //    //    return permissions;
    //    //}
    //}
    //internal static class IdentityExtensions
    //{
    //    public static IEnumerable<Permission> Reduce(this IEnumerable<Permission> me, Function function)
    //    {
    //        return me.Where(p =>
    //            // Tiene la misma funcion
    //            p.Function == function || // o
    //                                      // Es un permiso de denegacion y la funcion esta incluida
    //            (!p.Value && Functions.GetGraph().GetDescendants(function).Contains(p.Function)) || // o
    //                                                                                                // Es un permios de concesion y la funcion esta excluida
    //            (p.Value && Functions.GetGraph().GetAncesdants(function).Contains(p.Function))).ToList();
    //    }
    //}

}
