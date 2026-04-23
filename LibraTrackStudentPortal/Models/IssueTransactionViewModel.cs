using System.Collections.Generic;

namespace LibraTrackStudentPortal.Models
{
    public class IssueTransactionViewModel
    {
        public string StudentID { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string GradeSection { get; set; }
        public int ReservedCount { get; set; }
        public List<string> RequestNos { get; set; }
        public List<IssueBookDetailViewModel> Books { get; set; }
    }

    public class IssueBookDetailViewModel
    {
        public string BookId { get; set; }
        public string BookTitle { get; set; }
        public string Author { get; set; }
    }
}