using System;
using System.Collections.Generic;

namespace SmeLms.Models {
    public partial class Course {
        public string Coursecode { get; set; }
        public decimal Duration { get; set; }
        public string Title { get; set; }
        public string Catid { get; set; }
        public DateTime? Publisheddate { get; set; } = DateTime.Now;
        public decimal Price { get; set; }
        public string Status { get; set; } = "InDraft";
        public string Author { get; set; }
        public int Loyaltypoint { get; set; } = 0;
        public byte? Enforcesequence { get; set; } = 1;
        public byte Freeonsubcription { get; set; } = 1;
        public byte Showstudentcount { get; set; } = 0;
        public string Relatedcourses { get; set; }
        public string Description { get; set; }
        public string Tag { get; set; }
        public string Objectives { get; set; }
    }
}