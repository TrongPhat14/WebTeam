using System.ComponentModel.DataAnnotations.Schema;
using WebTeam.Data.Migrations;

namespace WebTeam.Data
{
    [Table("Comment")]

    public partial class Comment
    {
        public int CommentID { get; set; }

        public int? ArticleID { get; set; }

        public string CommentContent { get; set; }

        public DateTime CommentDate { get; set; }

        public string? UserID { get; set; }

        [ForeignKey("ArticleID")]
        public virtual Article? Article { get; set; }

        [ForeignKey("UserID")]
        public virtual ApplicationUser? User { get; set; }
    }
}
