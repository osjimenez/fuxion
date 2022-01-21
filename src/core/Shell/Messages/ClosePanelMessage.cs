namespace Fuxion.Shell.Messages; 

using Telerik.Windows.Controls;

internal record ClosePanelMessage(PanelName? Name = null, RadPane? Pane = null);