using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraTrackStudentPortal.Models
{
    [Table("users")]
    public class Users
    {
        [Key]
        public int id { get; set; }

        public string? id_number { get; set; }

        public string? full_name { get; set; }
        public string? email { get; set; }
        public string? username { get; set; }
        public string? password { get; set; }
        public DateTime? date_register { get; set; }
        public string? status { get; set; }
        public string? role { get; set; }
        public bool? IsFirstLogin { get; set; }
    }
}
