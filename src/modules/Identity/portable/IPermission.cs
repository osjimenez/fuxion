using System.Collections.Generic;
using System.Linq;
using Fuxion.Identity.Helpers;
using System.Reflection;

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
        internal static bool Match(this IPermission me, IFunction function, IDiscriminator[] discriminators)
        {
            //bool res = false;
            return Printer.Indent($"{nameof(PermissionExtensions)}.{nameof(Match)}:", () =>
            {
                Printer.Indent("Input parameters", () =>
                {
                    //Printer.Print($"Permission (me): {me}");
                    Printer.WriteLine($"Permission:");
                    new[] { me }.Print(PrintMode.Table);
                    Printer.WriteLine($"Function: {function.Name}");
                    Printer.WriteLine($"Discriminadores:");
                    discriminators.Print(PrintMode.Table);
                    //Printer.Foreach($"Discriminadores:", discriminators, dis => Printer.Print($"{dis.TypeName} - {dis.Name}"));
                });
                if (me.MatchByFunction(function))
                {
                    if (me.MatchByDiscriminatorsType(discriminators))
                    {
                        if (me.MatchByDiscriminatorsInclusionsAndExclusions(discriminators))
                        {
                            return true;
                        }
                        else
                        {
                            Printer.WriteLine($"Falló el matching al comprobar los inclusiones/exclusiones del discriminador");
                            return false;
                        }
                    }
                    else
                    {
                        Printer.WriteLine($"Falló el matching al comprobar el tipo de discriminador");
                        return false;
                    }
                }
                else
                {
                    Printer.WriteLine($"Falló el matching al comprobar la función");
                    return false;
                }
                //var byFunction = me.MatchByFunction(function);
                //var byDiscriminator = me.MatchByDiscriminatorsType(discriminators);
                //var byDiscriminatorPath = me.MatchByDiscriminatorsPath(discriminators);
                //res = byFunction && byDiscriminator && byDiscriminatorPath;
                //Printer.WriteLine($"Resultado del matching => byFunction<{byFunction}> && byDiscriminator<{byDiscriminator}> && byDiscriminatorPath<{byDiscriminatorPath}>: {res}");
            });
            //return res;
        }
        internal static bool MatchByFunction(this IPermission me, IFunction function)
        {
            return Printer.Indent($"{nameof(PermissionExtensions)}.{nameof(MatchByFunction)}:", () =>
            {
                Printer.Indent("Input parameters", () =>
                {
                    Printer.WriteLine($"Permission:");
                    new[] { me }.Print(PrintMode.Table);
                    Printer.WriteLine($"Function: {function.Name}");
                });
                Printer.WriteLine($"Inclusiones: {me.Function.GetAllInclusions().Aggregate("", (a, s) => a + " - " + s.Id, a => a.Trim(' ','-'))}");
                Printer.WriteLine($"Exclusiones: {me.Function.GetAllExclusions().Aggregate("", (a, s) => a + " - " + s.Id, a => a.Trim(' ', '-'))}");
                var comparer = new FunctionEqualityComparer();
                // Si es la misma función, TRUE.
                var byFunc = comparer.Equals(me.Function, function);
                // Si soy un permiso de concesión y la funcion esta incluida, TRUE.
                // Ejemplo: Soy un permiso que concede edición y la funcion que me piden es de lectura
                //          la edición implica/incluye la lectura, pro lo tanto, encaja.
                var byInclusion = me.Value && me.Function.GetAllInclusions().Contains(function, comparer);
                // Si soy un permiso de denegacion y la funcion esta excluida, TRUE.
                // Ejemplo: Soy un permiso que deniega la lectura y la funcion que me piden es de edición
                //          la lectura excluye la edición, si no puedo leer algo tampoco podré editarlo
                //          por lo tanto es permiso encaja.
                var byExclusion = !me.Value && me.Function.GetAllExclusions().Contains(function, comparer);
                var res = byFunc || byInclusion || byExclusion;
                Printer.WriteLine($"Resultado => byFunc<{byFunc}> || byInclusion<{byInclusion}> || byExclusion<{byExclusion}>: {res}");
                // El permiso nos dará la función por cualquiera de los trés métodos.
                return res;
            });
        }
        internal static bool MatchByDiscriminatorsType(this IPermission me, IEnumerable<IDiscriminator> discriminators)
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
            return Printer.Indent($"{nameof(PermissionExtensions)}.{nameof(MatchByDiscriminatorsType)}:", () =>
            {
                Printer.Indent("Input parameters", () =>
                {
                    //Printer.Print($"Permission (me): {me}");
                    Printer.WriteLine("Permission:");
                    new[] { me }.Print(PrintMode.Table);
                    //Printer.Foreach($"Discriminadores:", discriminators, dis => Printer.Print($"{dis.TypeName} - {dis.Name}"));
                    Printer.WriteLine("Discriminators:");
                    discriminators.Print(PrintMode.Table);
                });
                bool res = false;
                // Si no tiene ninguno de los tipos, no encaja.
                Printer.WriteLine("Or my permission haven't scopes or some of these scopes match by discriminator type with any of given scopes.");
                if (me.Scopes.Count() == 0)
                {
                    Printer.WriteLine($"Este permiso no tiene ningun scope");
                    res = true;
                }
                else
                {
                    var scos = me.Scopes.Where(s => discriminators.Select(d => d.TypeId).Contains(s.Discriminator.TypeId));
                    Printer.WriteLine($"Este permiso tiene '{scos.Count()}' scopes para los tipos de los discriminadores dados");
                    res = true;
                }
                //res = me.Scopes.Count() == 0 || me.Scopes.Any(s => discriminators.Select(d => d.TypeId).Contains(s.Discriminator.TypeId));
                Printer.WriteLine($"Result: {res}");
                return res;
            });
        }
        internal static bool MatchByDiscriminatorsInclusionsAndExclusions(this IPermission me, IEnumerable<IDiscriminator> discriminators)
        {
            //return Printer.Ident($"{nameof(PermissionExtensions)}.{nameof(MatchByDiscriminatorsPath)}:", () => {
            return Printer.Indent($"{typeof(PermissionExtensions).GetTypeInfo().DeclaredMethods.FirstOrDefault(m=>m.Name == nameof(MatchByDiscriminatorsInclusionsAndExclusions)).GetSignature()}:", () => {
                Printer.Indent("Input parameters", () =>
                {
                    //Printer.Print($"Permission (me): {me}");
                    Printer.WriteLine("Permission:");
                    new[] { me }.Print(PrintMode.Table);
                    //Printer.Foreach($"Discriminadores:", discriminators, dis => Printer.Print($"{dis.TypeName} - {dis.Name}"));
                    Printer.WriteLine("Discriminadores:");
                    discriminators.Print(PrintMode.Table);
                });
                //Printer.Foreach($"Permission scopes ({me.Scopes.Count()}):", me.Scopes, sco => Printer.Print(sco.ToString()));
                if (!me.Scopes.Any())
                {
                    
                    Printer.WriteLine($"Resultado: TRUE - No tengo scopes");
                    return true;
                }
                else
                {
                    return Printer.Indent("Analizamos cada scope:", () =>
                    {
                        // Tenemos que tomar nuestros discriminadores, y comprobarlos contra los discriminadores que me han pasado
                        // - Cojo un discriminador y busco el discriminador del mismo tipo en la entrada:
                        //    - No hay un discriminador del mismo tipo, pues no encaja
                        //    - Si hay un discriminador del mismo tipo, compruebo la ruta
                        return me.Scopes.Any(sco=> {
                            Printer.WriteLine($"Scope {sco}");
                            Printer.IndentationLevel++;
                            if (discriminators.Count(d => Comparer.AreEquals(d.TypeId, sco.Discriminator.TypeId)) == 1)
                            {
                                // Si hay un discriminador del mismo tipo, compruebo la ruta
                                var target = discriminators.Single(d => Comparer.AreEquals(d.TypeId, sco.Discriminator.TypeId));
                                Printer.WriteLine($"Se propaga a mi {sco.Propagation.HasFlag(ScopePropagation.ToMe)} ids = {target.Id}-{sco.Discriminator.Id}");
                                // Se propaga a mi y es el mismo discriminador
                                if (sco.Propagation.HasFlag(ScopePropagation.ToMe) && Comparer.AreEquals(target.Id, sco.Discriminator.Id))
                                {
                                    Printer.WriteLine($"Se propaga a mi y es el mismo discriminador");
                                    return true;
                                }
                                // Se propaga hacia arriba y su id esta en mi path:
                                //if (sco.Propagation.HasFlag(ScopePropagation.ToExclusions) && sco.Discriminator.Path.Contains(target.Id)) return true;
                                if (sco.Propagation.HasFlag(ScopePropagation.ToExclusions) && sco.Discriminator.GetAllExclusions().Contains(target))
                                {
                                    Printer.WriteLine($"Se propaga hacia arriba y su id esta en mi path");
                                    return true;
                                }
                                // Se propaga hacia abajo y mi id esta en su path:
                                //if (sco.Propagation.HasFlag(ScopePropagation.ToInclusions) && target.Path.Contains(sco.Discriminator.Id)) return true;
                                if (sco.Propagation.HasFlag(ScopePropagation.ToInclusions) && sco.Discriminator.GetAllInclusions().Contains(target))
                                {
                                    Printer.WriteLine($"Se propaga hacia abajo y mi id esta en su path");
                                    return true;
                                }
                                Printer.IndentationLevel--;
                                return false;
                            }
                            else
                            {
                                // No hay un discriminador del mismo tipo, pues no encaja
                                Printer.WriteLine($"No hay un discriminador del mismo tipo");
                                Printer.IndentationLevel--;
                                return true;
                            }
                        });
                    });
                }
            });
        }
        public static string ToOneLineString(this IPermission me)
        {
            return $"{me.Value} - {me.Function.Name} - {me.Scopes.Count()}";
        }
        public static void Print(this IEnumerable<IPermission> me, PrintMode mode)
        {
            switch (mode)
            {
                case PrintMode.OneLine:
                    foreach (var per in me)
                    {
                        Printer.WriteLine(per.Function.Name.PadRight(8, ' ') + " , v:" +
                            per.Value + "".PadRight(per.Value ? 1 : 0, ' ') + " , ss:[" +
                            per.Scopes.Aggregate("", (str, actual) => str + actual + ",", str => str.Trim(',')) +
                            "]");
                    }
                    break;
                case PrintMode.PropertyList:
                    break;
                case PrintMode.Table:
                    var valueLength = me.Select(p => p.Value.ToString().Length).Union(new[] { "VALUE".Length }).Max();
                    var functionLength = me.Select(p => p.Function.Name.ToString().Length).Union(new[] { "FUNCTION".Length }).Max();
                    var typeLength = new[] { "TYPE".Length }.Concat(me.SelectMany(p => p.Scopes.Select(s => s.Discriminator.TypeName.Length))).Max();
                    var nameLength = new[] { "NAME".Length }.Concat(me.SelectMany(p => p.Scopes.Select(s => s.Discriminator.Name.Length))).Max();
                    var propagationLength = new[] { "PROPAGATION".Length }.Concat(me.SelectMany(p => p.Scopes.Select(s => s.Propagation.ToString().Length))).Max();

                    Printer.WriteLine("┌" + ("".PadRight(valueLength, '─')) + "┬" + ("".PadRight(functionLength, '─')) + "┬" + ("".PadRight(typeLength, '─')) + "┬" + ("".PadRight(nameLength, '─')) + "┬" + ("".PadRight(propagationLength, '─')) + "┐");
                    if (me.Any())
                    {
                        Printer.WriteLine("│" + ("VALUE".PadRight(valueLength, ' ')) + "│" + ("FUNCTION".PadRight(functionLength, ' ')) + "│" + ("TYPE".PadRight(typeLength, ' ')) + "│" + ("NAME".PadRight(nameLength, ' ')) + "│" + ("PROPAGATION".PadRight(propagationLength, ' ')) + "│");
                        Printer.WriteLine("├" + ("".PadRight(valueLength, '─')) + "┼" + ("".PadRight(functionLength, '─')) + "┼" + ("".PadRight(typeLength, '─')) + "┼" + ("".PadRight(nameLength, '─')) + "┼" + ("".PadRight(propagationLength, '─')) + "┤");
                    }

                    foreach(var per in me)
                    {
                        var list = per.Scopes.ToList();
                        if (list.Count == 0)
                        {
                            Printer.WriteLine("│" +
                                    per.Value.ToString().PadRight(valueLength, ' ') + "│" +
                                    per.Function.Name.PadRight(functionLength, ' ') + "│" +
                                    ("".PadRight(typeLength, ' ')) + "│" +
                                    ("".PadRight(nameLength, ' ')) + "│" +
                                    ("".PadRight(propagationLength, ' ')) + "│");
                        }
                        else {
                            for (int i = 0; i < list.Count; i++)
                            {
                                Printer.WriteLine("│" +
                                    ((i == 0 ? per.Value.ToString() : "").PadRight(valueLength, ' ')) + "│" +
                                    ((i == 0 ? per.Function.Name : "").PadRight(functionLength, ' ')) + "│" +
                                    (list[i].Discriminator.TypeName.PadRight(typeLength, ' ')) + "│" +
                                    (list[i].Discriminator.Name.PadRight(nameLength, ' ')) + "│" +
                                    (list[i].Propagation.ToString().PadRight(propagationLength, ' ')) + "│");
                            }
                        }
                    }
                    Printer.WriteLine("└" + ("".PadRight(valueLength, '─')) + "┴" + ("".PadRight(functionLength, '─')) + "┴" + ("".PadRight(typeLength, '─')) + "┴" + ("".PadRight(nameLength, '─')) + "┴" + ("".PadRight(propagationLength, '─')) + "┘");
                    break;
            }
        }
    }
    public class PermissionEqualityComparer : IEqualityComparer<IPermission>
    {
        FunctionEqualityComparer funCom = new FunctionEqualityComparer();
        ScopeEqualityComparer scoCom = new ScopeEqualityComparer();
        public bool Equals(IPermission x, IPermission y)
        {
            return AreEquals(x, y);
        }

        public int GetHashCode(IPermission obj)
        {
            if (obj == null) return 0;
            return funCom.GetHashCode(obj.Function) ^ obj.Scopes.Select(s => scoCom.GetHashCode(s)).Aggregate(0, (a, c) => a ^ c) ^ obj.Value.GetHashCode();
        }
        bool AreEquals(object obj1, object obj2)
        {
            // If both are NULL, return TRUE
            if (Equals(obj1, null) && Equals(obj2, null)) return true;
            // If some of them is null, return FALSE
            if (Equals(obj1, null) || Equals(obj2, null)) return false;
            // If any of them are of other type, return FALSE
            if (!(obj1 is IPermission) || !(obj2 is IPermission)) return false;
            var per1 = (IPermission)obj1;
            var per2 = (IPermission)obj2;
            // Use 'Equals' to compare the ids
            return funCom.Equals(per1.Function, per2.Function) &&
                per1.Scopes.All(s => per2.Scopes.Any(s2 => scoCom.Equals(s, s2))) &&
                per2.Scopes.All(s => per1.Scopes.Any(s2 => scoCom.Equals(s, s2))) &&
                per1.Value == per2.Value;
        }
    }
}
