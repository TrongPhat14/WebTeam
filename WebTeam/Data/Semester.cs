using System.ComponentModel.DataAnnotations.Schema;

namespace WebTeam.Data
{
    [Table("Semester")]
    public partial class Semester
    {
        public int SemesterID { get; set; }
        public string SemesterName { get; set; }

        public string? Notification { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ClosureDate { get; set; }

        public DateTime? Dl1 { get; set; }

        public DateTime? DL2 { get; set; }

        public int? FacultyID { get; set; }

        public int? AcademicYearID { get; set; }


        [ForeignKey("FacultyID")]
        public virtual Faculty? Faculty { get; set; }


        [ForeignKey("AcademicYearID")]
        public virtual AcademicYear? AcademicYear { get; set; }

        public virtual ICollection<Article> Articles { get; set; } = new List<Article>();

    }
}
