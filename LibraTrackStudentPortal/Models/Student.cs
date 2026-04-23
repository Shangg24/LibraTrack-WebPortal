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
        public bool MustChangePassword { get; set; }

        public string? username { get; set; }
        public string? contact { get; set; }
        public string? grade_section { get; set; }
        public DateTime date_registered { get; set; }
    }
}