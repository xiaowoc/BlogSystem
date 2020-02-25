using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogSystem.DTO
{
   public  class ArticleDto
    {
        public Guid Id { get; set; }
        [Display(Name = "标题")]
        public string Title { get; set; }
        [Display(Name = "内容")]
        public string Content { get; set; }
        [Display(Name = "内容简介")]
        public string IntroContent { get; set; }
        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }
        [Display(Name = "邮箱")]
        public string Email { get; set; }
        [Display(Name ="用户id")]
        public Guid userId { get; set; }
        [Display(Name = "点赞数")]
        public int GoodCount { get; set; }
        [Display(Name = "反对数")]
        public int BadCount { get; set; }
        [Display(Name = "图片路径")]
        public string imagePath { get; set; }
        [Display(Name = "所属分类")]
        public string[] CategoryNames { get; set; }
        public Guid[] CategoryIds { get; set; }
    }
}
