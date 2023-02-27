using Fuxion.Telerik_.Wpf;
using Telerik.Windows.Controls;

namespace Fuxion.Telerik_.Wpf.Messages;

record ClosePanelMessage(PanelName? Name = null, RadPane? Pane = null);