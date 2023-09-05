using System.Runtime.CompilerServices;
using System.Windows.Markup;

[assembly: XmlnsPrefix("fuxion", "fuxion")]
[assembly: XmlnsDefinition("fuxion", "Fuxion.Windows.Data")]
[assembly: XmlnsDefinition("fuxion", "Fuxion.Windows.Markup")]
[assembly: XmlnsDefinition("fuxion", "Fuxion.Windows.Resources")]
[assembly: XmlnsDefinition("fuxion", "Fuxion.Windows.Helpers")]
[assembly: XmlnsDefinition("fuxion", "Fuxion.Windows.Controls")]
[assembly: InternalsVisibleTo("Fuxion.Windows.Test")]
#if DEBUG
[assembly: InternalsVisibleTo("DemoWpf")]
#endif