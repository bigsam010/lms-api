using System;
using System.Collections.Generic;

namespace SmeLms.Models
{
    public partial class Topiccontent
    {
        public int Contentid { get; set; }
        public int Topicid { get; set; }
        public string Content { get; set; }
        public string Contenttype { get; set; }
        public int Contentposition { get; set; }
        public string Fileformat { get; set; }
    }
}
