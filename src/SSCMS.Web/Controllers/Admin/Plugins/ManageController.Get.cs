﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;

namespace SSCMS.Web.Controllers.Admin.Plugins
{
    public partial class ManageController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get()
        {
            if (!await _authManager.HasAppPermissionsAsync(Types.AppPermissions.PluginsManagement))
            {
                return Unauthorized();
            }

            //var dict = await _pluginManager.GetPluginIdAndVersionDictAsync();
            //var list = dict.Keys.ToList();
            //var packageIds = Utilities.ToString(list);

            return new GetResult
            {
                Version = _settingsManager.Version,
                AllPlugins = _pluginManager.Plugins
            };
        }
    }
}
