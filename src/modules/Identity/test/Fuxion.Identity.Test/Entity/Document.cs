﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Document))]
    public abstract class Document : File
    {
        public IList<Writer> Authors { get; set; }

        [DiscriminatedBy(typeof(Circle))]
        public string CircleId { get; set; }
    }
}
