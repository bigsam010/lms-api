using System;
using System.Collections.Generic;

namespace SmeLms.Models
{
    public partial class Courselesson
    {
        public int Lessonid { get; set; }
        public string Coursecode { get; set; }
        public string Title { get; set; }
        public int Position { set; get; }
    }
}
