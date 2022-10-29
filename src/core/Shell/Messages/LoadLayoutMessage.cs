using System.IO;

namespace Fuxion.Shell.Messages;

record LoadLayoutMessage(Stream LayoutFileStream);