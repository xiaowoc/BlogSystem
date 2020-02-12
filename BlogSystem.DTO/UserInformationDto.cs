using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogSystem.DTO
{
    public class UserInformationDto
    {
        public Guid Id { get; set; }
        [Display(Name = "邮箱")]
        public string Email { get; set; }
        [Display(Name = "图片")]
        public string ImagePath { get; set; }
        [Display(Name = "网站名字")]
        public string SiteName { get; set; }
        [Display(Name = "粉丝数")]
        public int FansCount { get; set; }
        [Display(Name = "关注数")]
        public int FocusCount { get; set; }
    }
}
