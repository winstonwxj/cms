﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Enums;

namespace SSCMS.Web.Controllers.Admin.Wx
{
    public partial class LayerVideoController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<QueryResult>> Get([FromQuery] QueryRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                Types.SitePermissions.WxSend, Types.SitePermissions.WxReply))
            {
                return Unauthorized();
            }

            var groups = await _materialGroupRepository.GetAllAsync(MaterialType.Article);
            var count = await _materialVideoRepository.GetCountAsync(request.GroupId, request.Keyword);
            var videos = await _materialVideoRepository.GetAllAsync(request.GroupId, request.Keyword, request.Page, request.PerPage);

            return new QueryResult
            {
                Groups = groups,
                Count = count,
                Videos = videos
            };
        }
    }
}
