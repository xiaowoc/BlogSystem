using BlogSystem.IDAL;
using BlogSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogSystem.DAL
{
    public class LikeHateService:BaseService<LikeHate>,ILikeHateService
    {
        public LikeHateService():base(new BlogContext())
        {

        }
    }
}
