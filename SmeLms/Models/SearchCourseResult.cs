using System;
namespace SmeLms.Models {
    public class SearchCourseResult : Course {
        public int Numberofstudents { set; get; }
        public decimal Totalearnings { set; get; } = 0;
    }
}