﻿using System;
using System.ComponentModel.DataAnnotations;

namespace WCloud.Framework.Database.Abstractions.Entity
{
    /// <summary>
    /// 树结构
    /// </summary>
    public abstract class TreeEntityBase : BaseEntity
    {
        /// <summary>
        /// 默认值是1
        /// </summary>
        public const int FIRST_LEVEL = 1;

        /// <summary>
        /// 默认值是空字符串
        /// </summary>
        public const string FIRST_PARENT_UID = Lib.helper.StringHelper.Empty;

        /// <summary>
        /// 默认值是default
        /// </summary>
        public const string DEFAULT_GROUP = "default";

        /// <summary>
        /// 层级
        /// </summary>
        [Range(FIRST_LEVEL, FIRST_LEVEL + 500, ErrorMessage = "层级不在范围之内")]
        public virtual int Level { get; set; } = FIRST_LEVEL;

        /// <summary>
        /// 父级UID
        /// </summary>
        [StringLength(100, ErrorMessage = "父级UID长度错误")]
        public virtual string ParentUID { get; set; } = FIRST_PARENT_UID;

        /// <summary>
        /// 分组
        /// </summary>
        [StringLength(100, ErrorMessage = "分组长度错误")]
        public virtual string GroupKey { get; set; } = DEFAULT_GROUP;
    }
}
