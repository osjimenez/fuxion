﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Media))]
    public abstract class Media : File { }
}
