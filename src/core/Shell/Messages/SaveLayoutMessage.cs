﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Shell.Messages
{
	internal record SaveLayoutMessage(Stream LayoutFileStream);
}
