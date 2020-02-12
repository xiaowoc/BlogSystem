using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BlogSystem.MVCSite.Models.UserViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name ="电子邮件")]
        public string Email { get; set; }
        [Required]
        [StringLength(50,MinimumLength =6)]
        [Display(Name = "登陆密码")]
        [DataType(DataType.Password)]
        public string LoginPwd { get; set; }
        [Required]
        [Display(Name = "记住我")]
        public bool RemenberMe { get; set; }
    }
}