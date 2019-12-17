using System;
using System.Collections.Generic;

namespace SmeLms.Models
{
    public partial class Coursecategory
    {
        public int Catid { get; set; }
        public DateTime? Datecreated { get; set; }=DateTime.Now;
        public string Name { get; set; }
    }
}
