﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Dto;
using SSCMS.Utils;

namespace SSCMS.Web.Controllers.Admin.Cms.Contents
{
    public partial class ContentsController
    {
        [HttpPost, Route(RouteColumns)]
        public async Task<ActionResult<BoolResult>> Columns([FromBody] ColumnsRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                    Types.SitePermissions.Contents))
            {
                return Unauthorized();
            }

            var site = await _siteRepository.GetAsync(request.SiteId);
            if (site == null) return NotFound();

            var channel = await _channelRepository.GetAsync(request.ChannelId);
            channel.ListColumns = ListUtils.ToString(request.AttributeNames);

            await _channelRepository.UpdateAsync(channel);

            return new BoolResult
            {
                Value = true
            };
        }

        public class ColumnsRequest : ChannelRequest
        {
            public List<string> AttributeNames { get; set; }
        }
    }
}
