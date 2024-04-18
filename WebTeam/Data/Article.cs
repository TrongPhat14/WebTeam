using System.ComponentModel.DataAnnotations.Schema;
using WebTeam.Data.Migrations;

namespace WebTeam.Data
{
    [Table("Article")]
    public partial class Article
    {
        public int ArticleId { get; set; }

        public string? Title { get; set; }

        public string? Content { get; set; }

        public string? MagazineCover { get; set; }

        public string? AuthorID { get; set; }

        public int? CategoryId { get; set; }

        public int? FacultyId { get; set; }

        public int? SemesterId { get; set; }

        public DateTime? submissionDate { get; set; }

        public string? Comment { get; set; }


        public DateTime? ArticleDate { get; set; }

        public int NoOfLike { get; set; }

        public bool Isbool { get; set; }

        [ForeignKey("AuthorID")]
        public virtual ApplicationUser? Author { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [ForeignKey("FacultyId")]
        public virtual Faculty? Faculty { get; set; }

        [ForeignKey("SemesterId")]
        public virtual Semester? Semester { get; set; }

        public virtual ICollection<FeedBack> FeedBacks { get; set; } = new List<FeedBack>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}
