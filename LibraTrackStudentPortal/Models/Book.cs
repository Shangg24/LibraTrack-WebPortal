namespace LibraTrackStudentPortal.Models
{
    public class book
    {
        public string id { get; set; }
        public string book_title { get; set; }
        public string author { get; set; }
        public string Copies { get; set; }

        public string status { get; set; }

        public int available { get; set; }
    }
}
