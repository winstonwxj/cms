﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Core.Utils;
using SSCMS.Utils;

namespace SSCMS.Web.Controllers.Admin.Settings.Sites
{
    public partial class SitesSaveController
    {
        [HttpPost, Route(RouteSettings)]
        public async Task<ActionResult<SaveSettingsResult>> SaveSettings([FromBody] SaveRequest request)
        {
            if (!await _authManager.HasAppPermissionsAsync(Types.AppPermissions.SettingsSites))
            {
                return Unauthorized();
            }

            var caching = new CacheUtils(_cacheManager);
            var manager = new SiteTemplateManager(_pathManager, _databaseManager, caching);

            if (manager.IsSiteTemplateDirectoryExists(request.TemplateDir))
            {
                return this.Error("站点模板文件夹已存在，请更换站点模板文件夹！");
            }

            var site = await _siteRepository.GetAsync(request.SiteId);
            var sitePath = await _pathManager.GetSitePathAsync(site);
            var directoryNames = DirectoryUtils.GetDirectoryNames(sitePath);

            var directories = new List<string>();
            var siteDirList = await _siteRepository.GetSiteDirsAsync(0);
            foreach (var directoryName in directoryNames)
            {
                var isSiteDirectory = false;
                if (site.Root)
                {
                    foreach (var siteDir in siteDirList)
                    {
                        if (StringUtils.EqualsIgnoreCase(siteDir, directoryName))
                        {
                            isSiteDirectory = true;
                        }
                    }
                }
                if (!isSiteDirectory && !_pathManager.IsSystemDirectory(directoryName))
                {
                    directories.Add(directoryName);
                }
            }

            var files = DirectoryUtils.GetFileNames(sitePath);

            //var fileSystems = FileUtility.GetFileSystemInfoExtendCollection(await _pathManager.GetSitePathAsync(site));
            //foreach (FileSystemInfoExtend fileSystem in fileSystems)
            //{
            //    if (!fileSystem.IsDirectory) continue;

            //    var isSiteDirectory = false;
            //    if (site.Root)
            //    {
            //        foreach (var siteDir in siteDirList)
            //        {
            //            if (StringUtils.EqualsIgnoreCase(siteDir, fileSystem.Name))
            //            {
            //                isSiteDirectory = true;
            //            }
            //        }
            //    }
            //    if (!isSiteDirectory && !_pathManager.IsSystemDirectory(fileSystem.Name))
            //    {
            //        directories.Add(fileSystem.Name);
            //    }
            //}
            //foreach (FileSystemInfoExtend fileSystem in fileSystems)
            //{
            //    if (fileSystem.IsDirectory) continue;
            //    files.Add(fileSystem.Name);
            //}

            return new SaveSettingsResult
            {
                Directories = directories,
                Files = files
            };
        }
    }
}
