using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BlogSystem.MVCSite.Models.UserViewModels
{
    public class EditInfoViewModel
    {
        public Guid Id { get; set; }
        [Display(Name = "邮箱")]
        public string Email { get; set; }
        [Display(Name = "图片")]
        public string ImagePath { get; set; }
        [Display(Name = "昵称")]
        public string Nickname { get; set; }
        [Display(Name = "粉丝数")]
        public int FansCount { get; set; }
        [Display(Name = "关注数")]
        public int FocusCount { get; set; }
        [Display(Name = "博客图片")]
        [Required(ErrorMessage = "请上传你的博客图片！")]
        public HttpPostedFileBase Image { get; set; }
    }
}