﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Core.Utils.Serialization;
using SSCMS.Dto;

namespace SSCMS.Web.Controllers.Admin.Cms.Settings
{
    public partial class SettingsStyleChannelController
    {
        [HttpPost, Route(RouteExport)]
        public async Task<ActionResult<StringResult>> Export([FromBody] ChannelRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                Types.SitePermissions.SettingsStyleChannel))
            {
                return Unauthorized();
            }

            var channel = await _channelRepository.GetAsync(request.ChannelId);

            var fileName =
                await ExportObject.ExportRootSingleTableStyleAsync(_pathManager, _databaseManager, request.SiteId, _channelRepository.TableName,
                    _tableStyleRepository.GetRelatedIdentities(channel));

            var filePath = _pathManager.GetTemporaryFilesPath(fileName);
            var downloadUrl = _pathManager.GetRootUrlByPath(filePath);

            return new StringResult
            {
                Value = downloadUrl
            };
        }
    }
}
