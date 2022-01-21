namespace Fuxion.Shell.Messages;

internal record OpenPanelMessage(PanelName Name, Dictionary<string, object> Arguments);