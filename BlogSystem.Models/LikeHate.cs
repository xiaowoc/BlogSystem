using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogSystem.Models
{
    public class LikeHate:BaseEntity
    {
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        public User User { get; set; }

        [ForeignKey(nameof(Article))]
        public Guid ArticleId { get; set; }

        public Article Article { get; set; }

        public bool Like { get; set; }

        public bool Hate { get; set; }
    }
}
