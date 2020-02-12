using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogSystem.DTO
{
    public class BlogCategoryDto
    {
        public Guid Id { get; set; }
        [Display(Name = "文章类型")]
        public string BlogCategoryName { get; set; }
    }
}
