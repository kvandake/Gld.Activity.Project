﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using TomTeam.Project.Gld.Exam;
using TomTeam.Project.Gld.Exam.Dto;
using Abp.Authorization;
using TomTeam.Project.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Abp.Domain.Uow;
using Abp.AutoMapper;
using System.Data.Entity;
using Abp.Linq.Extensions;

namespace TomTeam.Project.Gld.Exam
{
    public class ProvincialExamAppService : TomAbpAppServiceBase, IProvincialExamAppService
    {

        IRepository<ExamTopic> _examTopicRepository;
        IRepository<Answer> _answerRepository;
        public ProvincialExamAppService(IRepository<ExamTopic> _examTopicRepository, IRepository<Answer> _answerRepository)
        {
            this._examTopicRepository = _examTopicRepository;
            this._answerRepository = _answerRepository;
        }

        [AbpAuthorize(AppPermissions.Pages_Activity_Manager)]
        [UnitOfWork]
        public async Task<int> AddOrUpdate(CreateOrUpdateExamInput input)
        {
            var examTopic = new ExamTopic();
            if (input.ExamTopic.Id.HasValue && input.ExamTopic.Id > 0)
            {
                examTopic = await _examTopicRepository.GetAsync(input.ExamTopic.Id.Value);
                await _answerRepository.DeleteAsync(x => x.ExamTopicId == input.ExamTopic.Id.Value);
            }
            input.ExamTopic.MapTo(examTopic);
            var id = await _examTopicRepository.InsertOrUpdateAndGetIdAsync(examTopic);
            string trueContent = string.Empty;
            //操作选项
            foreach (var item in input.Answers)
            {
                if (item.IsTrueAnswer)
                {
                    trueContent += item.Content + "；";
                }
                var answer = item.MapTo<Answer>();
                answer.ExamTopicId = id;
                await _answerRepository.InsertAsync(answer);
            }
            examTopic.AnswerContent = trueContent;
            examTopic.Id = id;
            await _examTopicRepository.UpdateAsync(examTopic);
            return id;
        }

        [AbpAuthorize(AppPermissions.Pages_Activity_Manager)]
        [UnitOfWork]
        public async Task DeleteProvincial(IdInput<int> input)
        {
            if (input.Id <= 0) throw new UserFriendlyException("数据传递错误");
            await _answerRepository.DeleteAsync(x => x.ExamTopicId == input.Id);
            await _examTopicRepository.DeleteAsync(input.Id);
        }

        public async Task<PagedResultOutput<ExamListDto>> GetExamList(SearchExamInput input)
        {
            var query = _examTopicRepository.GetAll()
                .Where(news => !news.IsDeleted);
            if (!string.IsNullOrEmpty(input.SearchTitle))
            {
                query = query.Where(x => x.TopicName.Contains(input.SearchTitle));
            }
            var listCount = await query.CountAsync();
            var list = await query.OrderByDescending(x => x.CreationTime).PageBy(input).ToListAsync();
            var dataListDto = list.MapTo<List<ExamListDto>>();
            return new PagedResultOutput<ExamListDto>(listCount, dataListDto);
        }

        [AbpAuthorize(AppPermissions.Pages_Activity_Manager)]
        public async Task<GetExamForEditOutput> GetExam(NullableIdInput input)
        {
            GetExamForEditOutput dataDetail = new GetExamForEditOutput();
            if (input.Id.HasValue && input.Id > 0)
            {
                var detail = await _examTopicRepository.GetAsync(input.Id.Value);
                dataDetail.ExamTopic = detail.MapTo<ExamTopicDto>();
                var answers =await _answerRepository.GetAllListAsync(x => x.ExamTopicId == input.Id.Value);
                dataDetail.Answers = answers.MapTo<List<AnswerDto>>();
            }
            return dataDetail;
        }

        /// <summary>
        /// 获取随机条数
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultOutput<ExamListDto>> GetExamRadom(SearchExamInput input)
        {
            var query = _examTopicRepository.GetAll().Include(x => x.Answers);
            if (!string.IsNullOrEmpty(input.SearchTitle))
            {
                query = query.Where(x => x.TopicName.Contains(input.SearchTitle));
            }
            var listCount = await query.CountAsync();
            query = query.OrderBy(x => Guid.NewGuid());
            var list = await query.PageBy(input).ToListAsync();
            var dataListDto = list.MapTo<List<ExamListDto>>();
            return new PagedResultOutput<ExamListDto>(listCount, dataListDto);
        }
    }
}
