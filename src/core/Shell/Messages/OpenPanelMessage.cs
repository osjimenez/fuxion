namespace Fuxion.Shell.Messages;

record OpenPanelMessage(PanelName Name, Dictionary<string, object> Arguments);