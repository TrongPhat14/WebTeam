using System.ComponentModel.DataAnnotations.Schema;

namespace WebTeam.Data
{
    [Table("Category")]
    public partial class Category
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = null!;

        public int? FacultyId { get; set; }

        [ForeignKey("FacultyId")]
        public virtual Faculty? Faculty { get; set; }

        public virtual ICollection<Article> Articles { get; set; } = new List<Article>();


    }
}
