using Telerik.Windows.Controls;

namespace Fuxion.Shell.Messages; 

record ClosePanelMessage(PanelName? Name = null, RadPane? Pane = null);