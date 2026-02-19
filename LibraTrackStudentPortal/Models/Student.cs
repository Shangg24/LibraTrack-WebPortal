using System.ComponentModel.DataAnnotations;

namespace LibraTrackStudentPortal.Models
{
    public class Student
    {
        [Key]
        public string ID_no { get; set; }

        [Required]
        public string full_name { get; set; }

        [Required]
        public string email { get; set; }

        [Required]
        public string password { get; set; }
    }
}
