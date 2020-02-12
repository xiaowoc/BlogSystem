using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BlogSystem.MVCSite.Models.ArticleViewModels
{
    public class CreateCommentViewModel
    {
        public Guid Id { get; set; }
        [Display(Name = "内容")]
        public string Content { get; set; }
    }
}