using System;
using System.Collections.Generic;

namespace SmeLms.Models
{
    public partial class Lessontopic
    {
        public int Topicid { get; set; }
        public string Title { get; set; }
        public int? Lessonid { get; set; }
        public int Position { set; get; }
    }
}
