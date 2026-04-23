using System.ComponentModel.DataAnnotations;

namespace LibraTrackStudentPortal.Models
{
    public class GradeSection
    {
        [Key]
        public int id { get; set; }
        public string department { get; set; }
        public string grade_level { get; set; }
        public string section { get; set; }
        public bool is_active { get; set; }
    }
}