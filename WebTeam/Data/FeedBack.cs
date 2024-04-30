using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebTeam.Data.Migrations;

namespace WebTeam.Data
{
    [Table("FeedBack")]
    public partial class FeedBack
    {
        public int FeedBackID { get; set; }

        public string? Content { get; set; }

        public DateTime? SentDate { get; set; }
        public bool IsCheckMes { get; set; }


        public string? Marketing_coordinatorID { get; set; }

        public int? ArticleId { get; set; }

        [ForeignKey("Marketing_coordinatorID")]
        public virtual ApplicationUser? Marketing_coordinator { get; set; }

        [ForeignKey("ArticleId")]
        public virtual Article? Article { get; set; }
    }
}
