using System;
using System.Collections.Generic;

namespace LibraTrackStudentPortal.Models
{
    public class RequestTransactionViewModel
    {
        public string RequestNo { get; set; }
        public string StudentID { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; }
        public int BookCount { get; set; }
        public List<RequestBookDetail> Books { get; set; }
    }

    public class RequestBookDetail
    {
        public int RequestId { get; set; }
        public string BookId { get; set; }
        public string BookTitle { get; set; }
        public string Author { get; set; }
    }
}