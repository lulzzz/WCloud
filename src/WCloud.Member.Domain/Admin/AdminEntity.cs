﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WCloud.Framework.Database.Abstractions.Entity;

namespace WCloud.Member.Domain.Admin
{
    public class AdminDto : IDto
    {
        public int Id { get; set; }
    }

    [Table("tb_admin")]
    public class AdminEntity : BaseEntity, IMemberShipDBTable, ILogicalDeletion, IUpdateTime, ILoginEntity
    {
        /// <summary>
        /// 用于登陆，唯一标志
        /// </summary>
        public virtual string UserName { get; set; }

        /// <summary>
        /// 昵称，用于展示
        /// </summary>
        public virtual string NickName { get; set; }

        /// <summary>
        /// md5加密的密码
        /// </summary>
        public virtual string PassWord { get; set; }

        /// <summary>
        /// 头像链接
        /// </summary>
        [StringLength(1000)]
        public virtual string UserImg { get; set; }

        public virtual string ContactPhone { get; set; }

        public virtual string ContactEmail { get; set; }

        public virtual DateTime? LastPasswordUpdateTimeUtc { get; set; }

        public virtual int Sex { get; set; }

        public virtual int IsDeleted { get; set; }

        [NotMapped]
        public virtual RoleEntity[] Roles { get; set; }
        public virtual DateTime? UpdateTimeUtc { get; set; }
    }
}
