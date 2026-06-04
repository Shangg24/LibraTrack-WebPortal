namespace LibraTrackStudentPortal.Models
{
    public class RecommendedBookViewModel
    {
        public string BookId { get; set; }

        public string BookTitle { get; set; }

        public int BorrowCount { get; set; }
        public string Tag { get; set; }
    }
}