﻿using System;
using System.Collections.Generic;
using System.Text;

namespace WCloud.Framework.Database.Dapper
{
    public interface ITableStructure
    {
        Type EntityType { get; }

        /// <summary>
        /// 表名
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// 主键
        /// </summary>
        Dictionary<string, string> PrimaryKeys { get; }

        /// <summary>
        /// 自动生成的字段
        /// </summary>
        Dictionary<string, string> AutoGeneratedColumns { get; }

        /// <summary>
        /// 普通字段
        /// </summary>
        Dictionary<string, string> GeneralColumns { get; }
    }
}
