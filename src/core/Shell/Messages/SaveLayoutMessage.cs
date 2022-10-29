using System.IO;

namespace Fuxion.Shell.Messages;

record SaveLayoutMessage(Stream LayoutFileStream);