﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SSCMS.Configuration;
using SSCMS.Core.Utils;
using SSCMS.Dto;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Web.Controllers.Admin.Cms.Contents
{
    [OpenApiIgnore]
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class ContentsLayerTagController : ControllerBase
    {
        private const string Route = "cms/contents/contentsLayerTag";

        private readonly IAuthManager _authManager;
        private readonly ISiteRepository _siteRepository;
        private readonly IChannelRepository _channelRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IContentTagRepository _contentTagRepository;

        public ContentsLayerTagController(IAuthManager authManager, ISiteRepository siteRepository, IChannelRepository channelRepository, IContentRepository contentRepository, IContentTagRepository contentTagRepository)
        {
            _authManager = authManager;
            _siteRepository = siteRepository;
            _channelRepository = channelRepository;
            _contentRepository = contentRepository;
            _contentTagRepository = contentTagRepository;
        }

        [HttpGet, Route(Route)]
        public async Task<ActionResult<ObjectResult<IEnumerable<string>>>> Get([FromQuery] ChannelRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                    Types.SitePermissions.Contents) ||
                !await _authManager.HasContentPermissionsAsync(request.SiteId, request.ChannelId, Types.ContentPermissions.Edit))
            {
                return Unauthorized();
            }

            var site = await _siteRepository.GetAsync(request.SiteId);
            if (site == null) return NotFound();

            var tagNames = await _contentTagRepository.GetTagNamesAsync(request.SiteId);

            return new ObjectResult<IEnumerable<string>>
            {
                Value = tagNames
            };
        }

        [HttpPost, Route(Route)]
        public async Task<ActionResult<BoolResult>> Submit([FromBody] SubmitRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                    Types.SitePermissions.Contents) ||
                !await _authManager.HasContentPermissionsAsync(request.SiteId, request.ChannelId, Types.ContentPermissions.Edit))
            {
                return Unauthorized();
            }

            var site = await _siteRepository.GetAsync(request.SiteId);
            if (site == null) return NotFound();

            var allTagNames = await _contentTagRepository.GetTagNamesAsync(request.SiteId);

            foreach (var tagName in request.TagNames)
            {
                if (!allTagNames.Contains(tagName))
                {
                    await _contentTagRepository.InsertAsync(request.SiteId, tagName);
                }
            }

            var summaries = ContentUtility.ParseSummaries(request.ChannelContentIds);
            foreach (var summary in summaries)
            {
                var channel = await _channelRepository.GetAsync(summary.ChannelId);
                var content = await _contentRepository.GetAsync(site, channel, summary.Id);
                if (content == null) continue;

                var list = new List<string>();
                foreach (var tagName in ListUtils.GetStringList(content.TagNames))
                {
                    if (allTagNames.Contains(tagName))
                    {
                        list.Add(tagName);
                    }
                }

                foreach (var name in request.TagNames)
                {
                    if (request.IsCancel)
                    {
                        if (list.Contains(name)) list.Remove(name);
                    }
                    else
                    {
                        if (!list.Contains(name)) list.Add(name);
                    }
                }
                content.TagNames = list;

                await _contentRepository.UpdateAsync(site, channel, content);
            }

            await _authManager.AddSiteLogAsync(request.SiteId, request.IsCancel ? "批量取消内容标签" : "批量设置内容标签");

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
