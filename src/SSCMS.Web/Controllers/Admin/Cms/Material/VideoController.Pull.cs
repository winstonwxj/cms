﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Enums;

namespace SSCMS.Web.Controllers.Admin.Cms.Material
{
    public partial class VideoController
    {
        [HttpPost, Route(RouteActionsPull)]
        public async Task<ActionResult<PullResult>> Pull([FromBody] PullRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                Types.SitePermissions.MaterialVideo))
            {
                return Unauthorized();
            }

            var (success, token, errorMessage) = await _openManager.GetAccessTokenAsync(request.SiteId);

            if (success)
            {
                await _openManager.PullMaterialAsync(token, MaterialType.Video, request.GroupId);
            }

            return new PullResult
            {
                Success = success,
                ErrorMessage = errorMessage
            };
        }
    }
}