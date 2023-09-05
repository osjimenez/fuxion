using Fuxion.Telerik_.Wpf;

namespace Fuxion.Telerik_.Wpf.Messages;

record OpenPanelMessage(PanelName Name, Dictionary<string, object> Arguments);