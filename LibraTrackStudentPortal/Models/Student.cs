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
        public string passwordHash { get; set; }
        public bool IsFirstLogin { get; set; }

        public bool IsActive { get; set; }
    }
}
