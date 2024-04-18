using System.ComponentModel.DataAnnotations.Schema;

namespace WebTeam.Data
{
    [Table("AcademicYear")]
    public partial class AcademicYear
    {
        public int AcademicYearID { get; set; }
        public string AcademicYears { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? ClosureDate { get; set; }

        public virtual ICollection<Semester> Semesters { get; set; } = new List<Semester>();

    }
}
