namespace LibraTrackStudentPortal.Models
{
    public class BookForecastViewModel
    {
        public string BookTitle { get; set; }

        public int CurrentBorrowCount { get; set; }

        public int PredictedNextMonth { get; set; }

        public string Trend { get; set; }
    }
}