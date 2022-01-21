namespace Fuxion.Shell;

using Fuxion.Shell.Messages;
using ReactiveUI;
using System.IO;
using System.Xml.Linq;
using Telerik.Windows.Controls;

public static class MessageBusExtensions
{
	private const string LAYOUT_FILE_PATH = "layout.xml";
	public static void LoadLayout(this IMessageBus me, string layoutFilePath = LAYOUT_FILE_PATH)
	{
		if (File.Exists(layoutFilePath))
			using (var str = File.OpenRead(layoutFilePath))
			{
				LoadLayout(me, str);
			}
	}
	public static void LoadLayout(this IMessageBus me, Stream layoutFileStream) => me.SendMessage(new LoadLayoutMessage(layoutFileStream));
	public static void SaveLayout(this IMessageBus me, string layoutFilePath = LAYOUT_FILE_PATH)
	{
		if (File.Exists(layoutFilePath)) File.Delete(layoutFilePath);
		using (var mem = new MemoryStream())
		{
			SaveLayout(me, mem);
			mem.Position = 0;
			File.AppendAllText(layoutFilePath, XDocument.Load(mem).ToString());
		}
	}
	public static void SaveLayout(this IMessageBus me, Stream layoutFileStream) => me.SendMessage(new SaveLayoutMessage(layoutFileStream));

	public static void OpenPanel(this IMessageBus me, PanelName name, params (string Key, object Value)[] args) => OpenPanel(me, name, args.Transform(list => new Dictionary<string, object>(list.Select(l => new KeyValuePair<string, object>(l.Key, l.Value)))));
	public static void OpenPanel(this IMessageBus me, PanelName name, Dictionary<string, object> args) => me.SendMessage(new OpenPanelMessage(name, args));
	internal static void OnOpenPanel(this IMessageBus me, Action<OpenPanelMessage> action) => me.Listen<OpenPanelMessage>().Subscribe(action);


	public static void ClosePanel(this IMessageBus me, PanelName name) => me.SendMessage(new ClosePanelMessage(name));
	internal static void ClosePanel(this IMessageBus me, RadPane pane) => me.SendMessage(new ClosePanelMessage(Pane: pane));
	internal static void OnClosePanel(this IMessageBus me, Action<ClosePanelMessage> action) => me.Listen<ClosePanelMessage>().Subscribe(action);

	public static void CloseAllPanels(this IMessageBus me) => me.SendMessage(new CloseAllPanelsMessage());
	internal static void OnCloseAllPanels(this IMessageBus me, Action<CloseAllPanelsMessage> action) => me.Listen<CloseAllPanelsMessage>().Subscribe(action);

	public static void CloseAllPanelsWithKey(this IMessageBus me, string key) => me.SendMessage(new CloseAllPanelsWithKeyMessage(key));
	internal static void OnCloseAllPanelsWithKey(this IMessageBus me, Action<CloseAllPanelsWithKeyMessage> action) => me.Listen<CloseAllPanelsWithKeyMessage>().Subscribe(action);



	public static void Lock(this IMessageBus me) => me.SendMessage(new LockMessage());
	public static void Unlock(this IMessageBus me) => me.SendMessage(new UnlockMessage());
}