﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SSCMS.Configuration;
using SSCMS.Models;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Web.Controllers.Admin.Common
{
    [OpenApiIgnore]
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class GroupChannelLayerAddController : ControllerBase
    {
        private const string Route = "common/groupChannelLayerAdd";

        private readonly IAuthManager _authManager;
        private readonly IChannelGroupRepository _channelGroupRepository;

        public GroupChannelLayerAddController(IAuthManager authManager, IChannelGroupRepository channelGroupRepository)
        {
            _authManager = authManager;
            _channelGroupRepository = channelGroupRepository;
        }

        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] GetRequest request)
        {
            var group = await _channelGroupRepository.GetAsync(request.SiteId, request.GroupId);

            return new GetResult
            {
                GroupName = group.GroupName,
                Description = group.Description
            };
        }

        [HttpPost, Route(Route)]
        public async Task<ActionResult<ListResult>> Add([FromBody] AddRequest request)
        {
            if (await _channelGroupRepository.IsExistsAsync(request.SiteId, request.GroupName))
            {
                return this.Error("保存失败，已存在相同名称的栏目组！");
            }

            var groupInfo = new ChannelGroup
            {
                SiteId = request.SiteId,
                GroupName = request.GroupName,
                Description = request.Description
            };

            await _channelGroupRepository.InsertAsync(groupInfo);

            await _authManager.AddSiteLogAsync(request.SiteId, "新增栏目组", $"栏目组:{groupInfo.GroupName}");

            var groups = await _channelGroupRepository.GetChannelGroupsAsync(request.SiteId);
            var groupNames = groups.Select(x => x.GroupName);

            return new ListResult
            {
                GroupNames = groupNames,
                Groups = groups
            };
        }

        [HttpPut, Route(Route)]
        public async Task<ActionResult<ListResult>> Edit([FromBody] EditRequest request)
        {
            var groupInfo = await _channelGroupRepository.GetAsync(request.SiteId, request.GroupId);

            if (groupInfo.GroupName != request.GroupName && await _channelGroupRepository.IsExistsAsync(request.SiteId, request.GroupName))
            {
                return this.Error("保存失败，已存在相同名称的栏目组！");
            }

            groupInfo.GroupName = request.GroupName;
            groupInfo.Description = request.Description;

            await _channelGroupRepository.UpdateAsync(groupInfo);

            await _authManager.AddSiteLogAsync(request.SiteId, "修改栏目组", $"栏目组:{groupInfo.GroupName}");

            var groups = await _channelGroupRepository.GetChannelGroupsAsync(request.SiteId);
            var groupNames = groups.Select(x => x.GroupName);

            return new ListResult
            {
                GroupNames = groupNames,
                Groups = groups
            };
        }
    }
}
