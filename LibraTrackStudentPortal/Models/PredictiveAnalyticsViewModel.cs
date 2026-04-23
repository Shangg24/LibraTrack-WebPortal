using System.Collections.Generic;

namespace LibraTrackStudentPortal.Models
{
    public class PredictiveAnalyticsViewModel
    {
        public int TotalBorrowedTransactions { get; set; }
        public int TotalRequestedBooks { get; set; }
        public int LowStockBooks { get; set; }
        public string MostBorrowedCategory { get; set; }

        public List<TopBookViewModel> TopBorrowedBooks { get; set; }
        public List<TopBookViewModel> TopRequestedBooks { get; set; }
        public List<BookDemandViewModel> DemandAlerts { get; set; }
    }

    public class TopBookViewModel
    {
        public string BookId { get; set; }
        public string BookTitle { get; set; }
        public int Count { get; set; }
    }

    public class BookDemandViewModel
    {
        public string BookId { get; set; }
        public string BookTitle { get; set; }
        public int RequestCount { get; set; }
        public int BorrowCount { get; set; }
        public int AvailableCopies { get; set; }
        public string DemandLevel { get; set; }
    }
}