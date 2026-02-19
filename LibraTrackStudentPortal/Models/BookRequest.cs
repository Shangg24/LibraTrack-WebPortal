using System.ComponentModel.DataAnnotations;

namespace LibraTrackStudentPortal.Models
{
    public class book_requests
    {
        [Key]
        public int request_id { get; set; }
        public string ID_no { get; set; }
        public string book_id { get; set; }
        public DateTime request_date { get; set; }
        public string status { get; set; }
    }

}
