using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;

[assembly: AssemblyTitle("Fuxion Windows")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

[assembly: XmlnsPrefix("fuxion", "fuxion")]
[assembly: XmlnsDefinition("fuxion", "Fuxion.Windows.Data")]
[assembly: XmlnsDefinition("fuxion", "Fuxion.Windows.Markup")]
[assembly: XmlnsDefinition("fuxion", "Fuxion.Windows.Resources")]
[assembly: XmlnsDefinition("fuxion", "Fuxion.Windows.Helpers")]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page, 
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page, 
                                              // app, or any theme specific resource dictionaries)
)]

[assembly: InternalsVisibleTo("Fuxion.Windows.Test")]