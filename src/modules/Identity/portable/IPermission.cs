using System.Collections.Generic;
using System.Linq;
using Fuxion.Identity.Helpers;
using System;

namespace Fuxion.Identity
{
    public interface IPermission
    {
        IFunction Function { get; }
        IEnumerable<IScope> Scopes { get; }
        bool Value { get; }
    }
    public static class PermissionExtensions
    {
        public static bool IsValid(this IPermission me) { return me.Function != null && me.Scopes.Select(s => s.Discriminator.TypeId).Distinct().Count() == me.Scopes.Count(); }
        public static bool Match(this IPermission me, IFunctionGraph functionCollection, IFunction function, IDiscriminator[] discriminators, Action<string, bool> console)
        {
            var byFunction = me.MatchByFunction(functionCollection, function, console);
            var byDiscriminator = me.MatchByDiscriminatorsType(discriminators, console);
            var byDiscriminatorPath = me.MatchByDiscriminatorsPath(discriminators, console);
            return
                byFunction &&
                byDiscriminator &&
                byDiscriminatorPath;
        }
        public static bool MatchByFunction(this IPermission me, IFunctionGraph functionGraph, IFunction function, Action<string, bool> console)
        {
            Action<string, bool> con = (m, i) => { if (console != null) console(m, i); };
            con($"Permiso '{me.Value}' coincidencia por función ... ({me.Function.Id} == {function.Id})", true);
            var comparer = new FunctionEqualityComparer();
            // Si es la misma función, TRUE.
            var byFunc = comparer.Equals(me.Function, function);

            con($"Mis inclusiones {functionGraph.GetIncludedBy(me.Function).Aggregate("", (a, s) => a + "-" + s.Id)}", true);

            // Si soy un permiso de concesión y la funcion esta incluida, TRUE.
            // Ejemplo: Soy un permiso que concede edición y la funcion que me piden es de lectura
            //          la edición implica/incluye la lectura, pro lo tanto, encaja.
            var byInclusion = me.Value && functionGraph.GetIncludedBy(me.Function).Contains(function, comparer);

            // Si soy un permiso de denegacion y la funcion esta excluida, TRUE.
            // Ejemplo: Soy un permiso que deniega la lectura y la funcion que me piden es de edición
            //          la lectura excluye la edición, si no puedo leer algo tampoco podré editarlo
            //          por lo tanto es permiso encaja.
            var byExclusion = !me.Value && functionGraph.GetExcludedBy(me.Function).Contains(function, comparer);

            con($"byFunc = {byFunc} byInclusion = {byInclusion} byExclusion = {byExclusion}", true);

            // El permiso nos dará la función por cualquiera de los trés métodos.
            return byFunc || byInclusion || byExclusion;
        }
        public static bool MatchByDiscriminatorsType(this IPermission me, IEnumerable<IDiscriminator> discriminators, Action<string, bool> console)
        {
            #region Notes
            // Tenemos que comprobar que todos los tipos de discriminadores que me han pasado estan presentes en este permiso, es decir
            // en los ambitos (Scopes) de este permiso.
            // Nota: El discriminador de tipo es un poco 'especial' y se hace difuso para entender los ejemplos usandolo, por lo tanto
            //       usaré otro tipo de discriminadores para el ejemplo
            // Ejemplo: 

            // Tenemos que comprobar si los tipos de los discriminadores estan presentes en este permiso

            //            EJEMPLOS
            //            1   2   3
            // SUPUESTOS ============
            //        1 | NO  NO  SI
            //        2 | NO  NO  SI
            //        3 | __  __  __
            // Ejemplo 1: Recibo discriminadores de los tipos 'Department' y 'Location', esto puede ser una verificación para una operación del tipo
            //          "Quiero editar una entidad del departamento  de ventas en la ciudad de madrid"
            //          SUPUESTO 1: Tengo un ambito (Scope) con el discriminador, 'TipoContrato'. No puedo discriminar el tipo de contrato porque no esta en la entrada.
            //             NO       El permiso de este supuesto podía querer dar permiso a un gestor de becarios para poder hacer lo que sea con las entidades de
            //                      becarios, pero si me piden hacer algo con un departamento de una ciudad, no puedo otrogar esa función porque no entiendo el contexto.
            //          SUPUESTO 2: Tengo dos discriminadores en los ambitos , uno 'TipoContrato' y otro 'Location'. Con esto, puedo discriminar la localización,
            //             NO       pero no puedo discriminar el tipo de contrato, por lo tanto no cumplo.
            //                      El permiso de este supuesto podría querer dar permiso a un gestor de becarios de una determianda ciudad, pero me piden permiso para 
            //                      hacer algo en un departamento de una ciudad, puedo discriminar la ciudad, pero no puedo decir que sea de un determinado tipo de contrato.
            //          SUPUESTO 3: Tengo un discriminador 'Location'. Ahora puedo discriminar la locaclización.
            //             SI       El permiso podría ser para un administrador de una ciudad, me piden hacer algo en un departamento de una ciudad y yo tengo permiso para 
            //                      hacer lo que sea en la ciudad, adelante.

            // Ejemplo 2: Recibo un discriminador de tipo 'Location', esto puede ser una verificación para una operación del tipo
            //            "Quiero editar lo que sea en la ciudad de madrid"
            //          SUPUESTO 1: Tengo un ambito (Scope) con el discriminador, 'TipoContrato'. No puedo discriminar el tipo de contrato porque no esta en la entrada.
            //             NO       El permiso de este supuesto podía querer dar permiso a un gestor de becarios para poder hacer lo que sea con las entidades de
            //                      becarios, pero si me piden hacer lo que sea en una ciudad, no puedo otrogar esa función porque no entiendo el contexto.
            //          SUPUESTO 2: Tengo dos discriminadores en los ambitos , uno 'TipoContrato' y otro 'Location'. Con esto, puedo discriminar la localización,
            //             NO       pero no puedo discriminar el tipo de contrato, por lo tanto no cumplo.
            //                      El permiso de este supuesto podría querer dar permiso a un gestor de becarios de una determianda ciudad, pero me piden permiso para 
            //                      lo que sea en una ciudad, puedo discriminar la ciudad, pero no puedo decir que sea de un determinado tipo de contrato.
            //          SUPUESTO 3: Tengo un discriminador 'Location'. Ahora puedo discriminar la locaclización.
            //             SI       El permiso podría ser para un administrador de una ciudad, me piden hacer lo que sea en una ciudad y yo tengo permiso para 
            //                      hacer lo que sea en la ciudad, adelante.

            // Ejemplo 3: Recibo un discriminador de tipo 'Department', esto puede ser una verificación para una operación del tipo
            //            "Quiero editar lo que sea en el departamento de ventas"
            //          SUPUESTO 1: Tengo un ambito (Scope) con el discriminador, 'TipoContrato'. No puedo discriminar el tipo de contrato porque no esta en la entrada.
            //             NO       El permiso de este supuesto podía querer dar permiso a un gestor de becarios para poder hacer lo que sea con las entidades de
            //                      becarios, pero si me piden hacer lo que sea en el departamento de ventas, no puedo otrogar esa función porque no entiendo el contexto.
            //          SUPUESTO 2: Tengo dos discriminadores en los ambitos , uno 'TipoContrato' y otro 'Location'. Con esto, no puedo discriminar nada, por lo tanto no cumplo.
            //             NO       El permiso de este supuesto podría querer dar permiso a un gestor de becarios de una determianda ciudad, pero me piden permiso para 
            //                      lo que sea en el departamento de ventas, no puedo otorgar la función porque no entiendo el contexto.
            //          SUPUESTO 3: Tengo un discriminador 'Location'. Ahora puedo discriminar la locaclización.
            //             NO       El permiso podría ser para un administrador de una ciudad, me piden hacer lo que sea en el departamento de ventas, no puedo darte permiso
            //                      para hacer lo que sea en la ciudad de Madrid.

            // CONCLUSION: Un permiso utiliza los discriminadores para clasificar las supuestas entradas, son como reglas AND, tendras permiso si X & Y & ...
            //             Si uno de esos criterios no se puede comprobar, yo no puede conceder el permiso.
            //             Se deberá conceder el permiso si todos mis discriminadores estan en los recibidos, auqnue haya recibido alguno que yo no tenga, pero los mios
            //             tienen que estar todos

            //             Si no ponemos ningún discriminador estamos asignando el permiso sin restricciones.
            //             Por ejemplo un permiso de lectura sin restricciones significaría que ese rol puede leer lo que sea en el sistema, por ejemplo para un rol de backup
            //             Otro ejemplo, un permiso de denegacion de la función de administración significaría que los usuarios de ese rol nunca podrán administrar ningñun
            //             objecto del sistema, podrán leer, o editar, pero no administrar, sería útil para limitar cuentas trabajadores externos o becarios.

            //
            //Si me han dado permiso para un determinado tipo de entidad
            //                      en una determianda localización, no puedo afirmar que tenga el permiso en un departamento dado.
            //                      
            #endregion
            Action<string, bool> con = (m, i) => { if (console != null) console(m, i); };
            con($"Buscando coincidencia por tipo de discriminatdor ... ", false);
            con($"Tengo {me.Scopes.Count()} scopes", true);
            // Si no tiene ninguno de los tipos, no encaja.
            var anyType = me.Scopes.All(s => discriminators.Select(d => d.TypeId).Contains(s.Discriminator.TypeId));
            //var anyType = me.Scopes.All(s => discriminators.Contains(s.Discriminator, new DiscriminatorEqualityComparer()));
            con($"resultado = {anyType}", true);
            return anyType;

            // Si alguno de los tipos esta, querrá decir que este permiso se aplica


            //    // Si alguno de los permisos no contiene ninguno de los discriminaodres, devuelvo true
            //    return !res.Any(p => p.Scopes.All(s => !discriminators.Select(d => d.TypeId).Contains(s.Discriminator.TypeId)));
            //return Scopes.All(s => !discriminators.Select(d => d.TypeId).Contains(s.Discriminator.TypeId));
        }
        public static bool MatchByDiscriminatorsPath(this IPermission me, IEnumerable<IDiscriminator> discriminators, Action<string, bool> console)
        {
            Action<string, bool> con = (m, i) => { if (console != null) console(m, i); };
            con($"Buscando coincidencia por ruta del discriminatdor:", true);
            // Tenemos que tomar nuestros discriminadores, y comprobarlos contra los discriminadores que me han pasado
            // - Cojo un discriminador y busco el discriminador del mismo tipo en la entrada:
            //    - No hay un discriminador del mismo tipo, pues no encaja
            //    - Si hay un discriminador del mismo tipo, compruebo la ruta
            foreach (var sco in me.Scopes)
            {
                con($"Scope {sco}", true);
                if (discriminators.Count(d => Comparer.AreEquals(d.TypeId, sco.Discriminator.TypeId)) == 1)
                {
                    // Si hay un discriminador del mismo tipo, compruebo la ruta
                    var target = discriminators.Single(d => Comparer.AreEquals(d.TypeId, sco.Discriminator.TypeId));
                    con($"Se propaga a mi {sco.Propagation.HasFlag(ScopePropagation.ToMe)} ids = {target.Id}-{sco.Discriminator.Id}", true);
                    // Se propaga a mi y es el mismo discriminador
                    if (sco.Propagation.HasFlag(ScopePropagation.ToMe) && Comparer.AreEquals(target.Id, sco.Discriminator.Id))
                    {
                        con($"Se propaga a mi y es el mismo discriminador", true);
                        return true;
                    }
                    // Se propaga hacia arriba y su id esta en mi path:
                    //if (sco.Propagation.HasFlag(ScopePropagation.ToExclusions) && sco.Discriminator.Path.Contains(target.Id)) return true;
                    if (sco.Propagation.HasFlag(ScopePropagation.ToExclusions) && sco.Discriminator.Exclusions.Contains(target.Id))
                    {
                        con($"Se propaga hacia arriba y su id esta en mi path", true);
                        return true;
                    }
                    // Se propaga hacia abajo y mi id esta en su path:
                    //if (sco.Propagation.HasFlag(ScopePropagation.ToInclusions) && target.Path.Contains(sco.Discriminator.Id)) return true;
                    if (sco.Propagation.HasFlag(ScopePropagation.ToInclusions) && sco.Discriminator.Inclusions.Contains(target.Id))
                    {
                        con($"Se propaga hacia abajo y mi id esta en su path", true);
                        return true;
                    }
                }
                else
                {
                    // No hay un discriminador del mismo tipo, pues no encaja
                    con($"No hay un discriminador del mismo tipo", true);
                    return false;
                }
            }
            return false;
        }
    }
}
