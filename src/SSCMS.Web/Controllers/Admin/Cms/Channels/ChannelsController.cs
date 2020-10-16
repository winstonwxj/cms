﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SSCMS.Configuration;
using SSCMS.Core.Utils;
using SSCMS.Dto;
using SSCMS.Enums;
using SSCMS.Models;
using SSCMS.Repositories;
using SSCMS.Services;

namespace SSCMS.Web.Controllers.Admin.Cms.Channels
{
    [OpenApiIgnore]
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class ChannelsController : ControllerBase
    {
        private const string Route = "cms/channels/channels";
        private const string RouteGet = "cms/channels/channels/{siteId:int}/{channelId:int}";
        private const string RouteAppend = "cms/channels/channels/actions/append";
        private const string RouteUpload = "cms/channels/channels/actions/upload";
        private const string RouteImport = "cms/channels/channels/actions/import";
        private const string RouteExport = "cms/channels/channels/actions/export";
        private const string RouteOrder = "cms/channels/channels/actions/order";

        private readonly ICacheManager<CacheUtils.Process> _cacheManager;
        private readonly IAuthManager _authManager;
        private readonly IPathManager _pathManager;
        private readonly ICreateManager _createManager;
        private readonly IDatabaseManager _databaseManager;
        private readonly ISiteRepository _siteRepository;
        private readonly IChannelRepository _channelRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IChannelGroupRepository _channelGroupRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly ITableStyleRepository _tableStyleRepository;

        public ChannelsController(ICacheManager<CacheUtils.Process> cacheManager, IAuthManager authManager, IPathManager pathManager, ICreateManager createManager, IDatabaseManager databaseManager, ISiteRepository siteRepository, IChannelRepository channelRepository, IContentRepository contentRepository, IChannelGroupRepository channelGroupRepository, ITemplateRepository templateRepository, ITableStyleRepository tableStyleRepository)
        {
            _cacheManager = cacheManager;
            _authManager = authManager;
            _pathManager = pathManager;
            _createManager = createManager;
            _databaseManager = databaseManager;
            _siteRepository = siteRepository;
            _channelRepository = channelRepository;
            _contentRepository = contentRepository;
            _channelGroupRepository = channelGroupRepository;
            _templateRepository = templateRepository;
            _tableStyleRepository = tableStyleRepository;
        }

        public class ChannelsResult
        {
            public Cascade<int> Channel { get; set; }
            public IEnumerable<string> IndexNames { get; set; }
            public IEnumerable<string> GroupNames { get; set; }
            public IEnumerable<Template> ChannelTemplates { get; set; }
            public IEnumerable<Template> ContentTemplates { get; set; }
        }

        public class ChannelResult
        {
            public Entity Entity { get; set; }
            public IEnumerable<Select<string>> LinkTypes { get; set; }
            public IEnumerable<Select<string>> TaxisTypes { get; set; }
            public IEnumerable<InputStyle> Styles { get; set; }
            public bool IsTemplateEditable { get; set; }
        }

        public class ImportRequest : ChannelRequest
        {
            public string FileName { get; set; }
            public bool IsOverride { get; set; }
        }

        public class OrderRequest : ChannelRequest
        {
            public int ParentId { get; set; }
            public int Taxis { get; set; }
            public bool IsUp { get; set; }
        }

        public class ChannelIdsRequest : SiteRequest
        {
            public List<int> ChannelIds { get; set; }
        }

        public class DeleteRequest : SiteRequest
        {
            public int ChannelId { get; set; }
            public string ChannelName { get; set; }
            public bool DeleteFiles { get; set; }
        }

        public class PutRequest : Entity
        {
            public int SiteId { get; set; }
            public string ChannelName { get; set; }
            public string IndexName { get; set; }
            public List<string> GroupNames { get; set; }
            public string Content { get; set; }
            public int ChannelTemplateId { get; set; }
            public int ContentTemplateId { get; set; }
            public string LinkUrl { get; set; }
            public LinkType LinkType { get; set; }
            public TaxisType DefaultTaxisType { get; set; }
            public string FilePath { get; set; }
            public string ChannelFilePathRule { get; set; }
            public string ContentFilePathRule { get; set; }
            public string Keywords { get; set; }
            public string Description { get; set; }
        }

        public class AppendRequest : SiteRequest
        {
            public int ParentId { get; set; }
            public int ChannelTemplateId { get; set; }
            public int ContentTemplateId { get; set; }
            public bool IsParentTemplates { get; set; }
            public bool IsIndexName { get; set; }
            public string Channels { get; set; }
        }

        private async Task<List<InputStyle>> GetInputStylesAsync(Channel channel)
        {
            var styles = new List<InputStyle>
            {
                new InputStyle
                {
                    AttributeName = nameof(Site.ImageUrl),
                    DisplayName = "栏目图片",
                    InputType = InputType.Image
                }
            };
            var tableStyles = await _tableStyleRepository.GetChannelStylesAsync(channel);
            styles.AddRange(tableStyles.Select(x => new InputStyle(x)));

            return styles;
        }
    }
}
