﻿using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomTeam.Project.Gld.Exam
{
    /// <summary>
    /// 用户考试信息汇总表
    /// </summary>
    public class ExamCollect : FullAuditedEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 乡试总得分
        /// </summary>
        public int ProvincialIntegral { get; set; }

        /// <summary>
        /// 用户是否完成考试
        /// </summary>
        public bool IsCompleteProvincial { get; set; }

        /// <summary>
        /// 是否有资格参加会试
        /// </summary>
        public bool IsMetropolitanStatus { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserDisplayName { get; set; }

        public List<ExamHistory> ExamHistory { get; set; }
    }
}
