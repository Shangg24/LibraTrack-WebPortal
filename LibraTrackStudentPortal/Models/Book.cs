using System;
using System.ComponentModel.DataAnnotations;

namespace LibraTrackStudentPortal.Models
{
    public class book
    {
        [Key]
        public string id { get; set; }

        public string? book_title { get; set; }
        public string? author { get; set; }
        public DateTime? published_date { get; set; }
        public string? status { get; set; }
        public DateTime? date_insert { get; set; }
        public DateTime? date_update { get; set; }
        public DateTime? date_delete { get; set; }
        public string? image { get; set; }
        public string? category { get; set; }
        public string? ISBN { get; set; }
        public string? shelf { get; set; }
        public string? BookID { get; set; }

        public int? Copies { get; set; }
        public int? available { get; set; }
        public int BookPK { get; set; }
    }
}