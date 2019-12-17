using System;
using System.Collections.Generic;

namespace SmeLms.Models {
    public partial class Blogpost {
        public int Postid { get; set; }
        public int? Views { get; set; }=0;
        public DateTime? Publisheddate { get; set; } = DateTime.Now;
        public string Author { get; set; }
        public string Tag { get; set; }
        public string Content { get; set; }
        public string Caption { get; set; }
        public string Status { get; set; }
        public int Shares { set; get; }
    }
}