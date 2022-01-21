namespace Fuxion.Shell.Messages;

using System.IO;

internal record SaveLayoutMessage(Stream LayoutFileStream);