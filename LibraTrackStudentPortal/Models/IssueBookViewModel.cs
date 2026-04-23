using System;

namespace LibraTrackStudentPortal.Models
{
    public class IssueBookViewModel
    {
        public int RequestId { get; set; }
        public string StudentID { get; set; }
        public string BookId { get; set; }
        public string BookTitle { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; }
    }
}