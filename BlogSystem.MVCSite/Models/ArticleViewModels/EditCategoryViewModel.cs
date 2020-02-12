using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BlogSystem.MVCSite.Models.ArticleViewModels
{
    public class EditCategoryViewModel
    {
        public Guid Id { get; set; }
        [Required]
        [StringLength(200, MinimumLength = 2)]
        [Display(Name = "分类名称")]
        public string CategoryName { get; set; }
    }
}