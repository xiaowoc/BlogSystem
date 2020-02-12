﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogSystem.Models
{
    public class BaseEntity
    {
        /// <summary>
        /// 编号
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 是否删除（伪删除）
        /// </summary>
        public bool IsRemoved { get; set; }
    }
}
