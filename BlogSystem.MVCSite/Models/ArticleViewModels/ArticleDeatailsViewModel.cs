using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BlogSystem.MVCSite.Models.ArticleViewModels
{
    /// <summary>
    /// 如果ViewModel和Dto中的类一样，则可以使用Dto的类。如果不同必须创建
    /// </summary>
    public class ArticleDeatailsViewModel
    {
        public Guid Id { get; set; }
        [Display(Name = "标题")]
        public string Title { get; set; }
        [Display(Name = "内容")]
        public string Content { get; set; }
        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }
        [Display(Name = "邮箱")]
        public string Email { get; set; }
        [Display(Name = "图片路径")]
        public string ImagePath { get; set; }
        public string[] CategoryIds { get; set; }
        [Display(Name = "所属分类")]
        public string[] CategoryNames { get; set; }
        [Display(Name = "点赞数")]
        public int GoodCount { get; set; }
        [Display(Name = "反对数")]
        public int BadCount { get; set; }
    }
}