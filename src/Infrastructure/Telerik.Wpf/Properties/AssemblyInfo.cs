using System.Windows.Markup;

[assembly: XmlnsPrefix("fuxion", "fuxion")]
[assembly: XmlnsDefinition("fuxion", "Fuxion.Telerik.Wpf")]
[assembly: XmlnsDefinition("fuxion", "Fuxion.Telerik.Wpf.Views")]

// TODO Pending to change namespace Fuxion.Telerik_.Wpf to Fuxion.Telerik.Wpf
// for now, I have a collision in XAML
// Posible solutions:
// https://stackoverflow.com/questions/7912573/wpf-globally-add-xaml-namespace-declaration
// https://github.com/dotnet/wpf/issues/7173
// https://github.com/dotnet/wpf/pull/7196