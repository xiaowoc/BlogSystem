using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogSystem.DTO
{
   public class CommentDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ArticleId { get; set; }
        public string Email { get; set; }
        public string Content { get; set; }
        public DateTime CreateTime { get; set; }
        public string ImagePath { get; set; }
    }
}
