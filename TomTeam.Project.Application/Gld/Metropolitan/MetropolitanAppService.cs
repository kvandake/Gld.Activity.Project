﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using TomTeam.Project.Gld.Metropolitan;
using TomTeam.Project.Gld.Metropolitan.Dto;
using Abp.Authorization;
using TomTeam.Project.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using Abp.AutoMapper;
using System.Data.Entity;
using Abp.Linq.Extensions;
using Abp.Domain.Uow;

namespace TomTeam.Project.Gld.Metropolitan
{
    public class MetropolitanAppService : TomAbpAppServiceBase, IMetropolitanAppService
    {
        private static object objLock = new object();
        IRepository<Exam.Metropolitan> _metropolitanRepository;
        IRepository<Exam.LikeInfo> _likeInfoRepository;
        public MetropolitanAppService(IRepository<Exam.Metropolitan> _metropolitanRepository, IRepository<Exam.LikeInfo> _likeInfoRepository)
        {
            this._metropolitanRepository = _metropolitanRepository;
            this._likeInfoRepository = _likeInfoRepository;
        }

        /// <summary>
        /// 投票
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<int> Vote(IdInput input)
        {
            if (!AbpSession.UserId.HasValue) throw new UserFriendlyException("没有获取到当前登录用户信息");
            var metropolitanModel = await _metropolitanRepository.FirstOrDefaultAsync(x => x.Id == input.Id);
            if (metropolitanModel == null || metropolitanModel.Id == 0)
                throw new UserFriendlyException("没有获取到该工程信息");
            var voteHistory = await _likeInfoRepository.GetAllListAsync(x => x.CreatorUserId == AbpSession.UserId.Value && x.MetropolitanId == metropolitanModel.Id);
            if (voteHistory.Count > 0)
                throw new UserFriendlyException("您已投过票，不可重复");
            await _likeInfoRepository.InsertAsync(new Exam.LikeInfo
            {
                MetropolitanId = metropolitanModel.Id
            });
            metropolitanModel.LikeCount = metropolitanModel.LikeCount + 1;
            await _metropolitanRepository.UpdateAsync(metropolitanModel);
            return metropolitanModel.LikeCount;
        }

        /// <summary>
        /// 上传工程
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task AddOrUpdateMetropolitian(CreateOrUpdateMetropolitanInput input)
        {
            if (!AbpSession.UserId.HasValue)
                throw new UserFriendlyException("没有获取到当前登录用户信息");
            var model = new Exam.Metropolitan();
            var selfMetropolitan = await _metropolitanRepository.FirstOrDefaultAsync(x => x.CreatorUserId == AbpSession.UserId.Value) ?? new Exam.Metropolitan();
            if (input.Id > 0)
            {
                var newsModel = await _metropolitanRepository.GetAsync(input.Id);
                if (newsModel.CreatorUserId == AbpSession.UserId.Value)
                {
                    input.MapTo(newsModel);
                    await _metropolitanRepository.InsertOrUpdateAndGetIdAsync(newsModel);
                }
                else
                {
                    throw new UserFriendlyException("您没有权限操作此工程");
                }
            }
            else
            {
                if (selfMetropolitan.Id > 0)
                {
                    throw new UserFriendlyException("您已上传过工程，请编辑您自己的工程");
                }
                input.MapTo(model);
                var currentUserInfo = await UserManager.FindByIdAsync(AbpSession.UserId.Value);
                model.UserDisplayName = currentUserInfo.Name;
                await _metropolitanRepository.InsertOrUpdateAndGetIdAsync(model);
            }
        }

        /// <summary>
        /// 删除工程
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [AbpAuthorize(AppPermissions.Pages_Activity_Manager)]
        public async Task DeleteMetropolitan(IdInput<int> input)
        {
            if (input.Id <= 0)
            {
                throw new UserFriendlyException("请传入正确的数值！");
            }
            await _metropolitanRepository.DeleteAsync(input.Id);
        }
        /// <summary>
        /// 获取工程信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<GetMetropolitanOutput> GetMetropolitanById(NullableIdInput input)
        {
            GetMetropolitanOutput newsDetail;
            if (input.Id.HasValue)
            {
                var news = await _metropolitanRepository.GetAsync(input.Id.Value);
                newsDetail = news.MapTo<GetMetropolitanOutput>();
            }

            else
            {
                newsDetail = new GetMetropolitanOutput();
            }
            return newsDetail;
        }
        /// <summary>
        /// 获取工程信息列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultOutput<GetMetropolitanOutput>> GetMetropolitanList(SearchMetropolitanInput input)
        {
            var query = _metropolitanRepository.GetAll().Where(news => !news.IsDeleted);
            if (!string.IsNullOrEmpty(input.SearchTitle))
            {
                query = query.Where(news => news.Title.Contains(input.SearchTitle));
            }
            var listCount = await query.CountAsync();
            var list = await query.OrderByDescending(x => x.CreationTime).PageBy(input).ToListAsync();
            var newsListDto = list.MapTo<List<GetMetropolitanOutput>>();
            return new PagedResultOutput<GetMetropolitanOutput>(listCount, newsListDto);
        }

        /// <summary>
        /// 更新工程信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [AbpAuthorize(AppPermissions.Pages_Activity_MetropolitanExamination)]
        public async Task<int> Update(UpdateMetropolitanForAdminInput input)
        {
            if (input.Id > 0)
            {
                var newsModel = _metropolitanRepository.Get(input.Id);
                input.MapTo(newsModel);
                return await _metropolitanRepository.InsertOrUpdateAndGetIdAsync(newsModel);
            }
            else
            {
                throw new UserFriendlyException("数据传递错误");
            }
        }

    }
}
