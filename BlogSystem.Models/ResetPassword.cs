using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogSystem.Models
{
    public class ResetPassword : BaseEntity
    {
        public string Token { get; set; }

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public bool IsSuccess { get; set; } = false;
        public DateTime ExpireTime { get; set; } = DateTime.Now.AddMinutes(10);
        public User User { get; set; }
    }
}
