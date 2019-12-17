using System;
using System.Collections.Generic;

namespace SmeLms.Models
{
    public partial class Classprogress
    {
        public int Id { get; set; }
        public int? Classid { get; set; }
        public int Contentcompleted { get; set; }
    }
}
