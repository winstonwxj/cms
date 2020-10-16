using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SSCMS.Configuration;
using SSCMS.Models;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Core.Services
{
    public partial class PathManager : IPathManager
    {
        private readonly ICacheManager<string> _cacheManager;
        private readonly ISettingsManager _settingsManager;
        private readonly IDatabaseManager _databaseManager;
        private readonly ISpecialRepository _specialRepository;
        private readonly ITemplateLogRepository _templateLogRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly IChannelRepository _channelRepository;
        private readonly IContentRepository _contentRepository;
        private readonly ITableStyleRepository _tableStyleRepository;

        public PathManager(ICacheManager<string> cacheManager, ISettingsManager settingsManager, IDatabaseManager databaseManager, ISpecialRepository specialRepository, ITemplateLogRepository templateLogRepository, ITemplateRepository templateRepository, ISiteRepository siteRepository, IChannelRepository channelRepository, IContentRepository contentRepository, ITableStyleRepository tableStyleRepository)
        {
            _cacheManager = cacheManager;
            _settingsManager = settingsManager;
            _databaseManager = databaseManager;
            _specialRepository = specialRepository;
            _templateLogRepository = templateLogRepository;
            _templateRepository = templateRepository;
            _siteRepository = siteRepository;
            _channelRepository = channelRepository;
            _contentRepository = contentRepository;
            _tableStyleRepository = tableStyleRepository;
        }

        //public string ApplicationPath => StringUtils.TrimEnd(_httpContext.Request.PathBase.Value, Constants.ApiPrefix);

        public string ContentRootPath => _settingsManager.ContentRootPath;
        public string WebRootPath => _settingsManager.WebRootPath;

        public string GetAdminUrl(params string[] paths)
        {
            return PageUtils.Combine($"/{Constants.AdminDirectory}", PageUtils.Combine(paths), "/");
        }

        public string GetHomeUrl(params string[] paths)
        {
            return PageUtils.Combine($"/{Constants.HomeDirectory}", PageUtils.Combine(paths), "/");
        }

        public string GetUploadFileName(string fileName)
        {
            var dt = DateTime.Now;
            return $"{dt.Day}{dt.Hour}{dt.Minute}{dt.Second}{dt.Millisecond}{PathUtils.GetExtension(fileName)}";
        }

        public async Task<string> GetWebUrlAsync(Site site)
        {
            return site.IsSeparatedWeb ? site.SeparatedWebUrl : await GetLocalSiteUrlAsync(site);
        }

        public async Task<string> GetAssetsUrlAsync(Site site)
        {
            return site.IsSeparatedAssets
                ? site.SeparatedAssetsUrl
                : PageUtils.Combine(await GetWebUrlAsync(site), site.AssetsDir);
        }

        public async Task UploadAsync(IFormFile file, string filePath)
        {
            DirectoryUtils.CreateDirectoryIfNotExists(filePath);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        public async Task UploadAsync(byte[] bytes, string filePath)
        {
            DirectoryUtils.CreateDirectoryIfNotExists(filePath);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await stream.WriteAsync(bytes);
        }
    }
}