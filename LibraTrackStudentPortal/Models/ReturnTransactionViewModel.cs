using System;
using System.Collections.Generic;

namespace LibraTrackStudentPortal.Models
{
    public class ReturnTransactionViewModel
    {
        public string IssueId { get; set; }
        public string StudentID { get; set; }
        public string StudentName { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public string Status { get; set; }
        public int BookCount { get; set; }

        public string ReturnIndicator { get; set; }
        public int DaysOverdue { get; set; }

        public List<ReturnBookDetail> Books { get; set; }
    }

    public class ReturnBookDetail
    {
        public string BookId { get; set; }
        public string BookTitle { get; set; }
        public string Author { get; set; }
    }
}