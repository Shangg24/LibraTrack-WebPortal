using System;

namespace LibraTrackStudentPortal.Models
{
    public class BorrowedBooksViewModel
    {
        public string BookTitle { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; }
    }
}
