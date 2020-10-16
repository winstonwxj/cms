﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;

namespace SSCMS.Web.Controllers.Admin.Cms.Settings
{
    public partial class SettingsStyleSiteController
    {
        [HttpDelete, Route(Route)]
        public async Task<ActionResult<DeleteResult>> Delete([FromBody] DeleteRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                Types.SitePermissions.SettingsStyleSite))
            {
                return Unauthorized();
            }

            await _tableStyleRepository.DeleteAsync(_siteRepository.TableName, request.SiteId, request.AttributeName);

            var tableName = _siteRepository.TableName;
            var relatedIdentities = _tableStyleRepository.GetRelatedIdentities(request.SiteId);
            var styles = await _tableStyleRepository.GetTableStylesAsync(tableName, relatedIdentities);

            return new DeleteResult
            {
                Styles = styles
            };
        }
    }
}
