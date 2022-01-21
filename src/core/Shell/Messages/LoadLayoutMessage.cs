namespace Fuxion.Shell.Messages;

using System.IO;

internal record LoadLayoutMessage(Stream LayoutFileStream);