﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SSCMS.Configuration;
using SSCMS.Dto;
using SSCMS.Enums;
using SSCMS.Models;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Web.Controllers.Admin.Common.Form
{
    [OpenApiIgnore]
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class LayerFileUploadController : ControllerBase
    {
        private const string Route = "common/form/layerFileUpload";
        private const string RouteUpload = "common/form/layerFileUpload/actions/upload";

        private readonly IPathManager _pathManager;
        private readonly ISiteRepository _siteRepository;
        private readonly IMaterialFileRepository _materialFileRepository;

        public LayerFileUploadController(IPathManager pathManager, ISiteRepository siteRepository, IMaterialFileRepository materialFileRepository)
        {
            _pathManager = pathManager;
            _siteRepository = siteRepository;
            _materialFileRepository = materialFileRepository;
        }

        [HttpGet, Route(Route)]
        public async Task<ActionResult<Options>> Get([FromQuery] SiteRequest request)
        {
            var site = await _siteRepository.GetAsync(request.SiteId);
            if (site == null) return this.Error("无法确定内容对应的站点");

            var options = TranslateUtils.JsonDeserialize(site.Get<string>(nameof(LayerFileUploadController)), new Options
            {
                IsChangeFileName = true,
                IsLibrary = true,
            });

            return options;
        }

        [RequestSizeLimit(long.MaxValue)]
        [HttpPost, Route(RouteUpload)]
        public async Task<ActionResult<UploadResult>> Upload([FromQuery] UploadRequest request, [FromForm] IFormFile file)
        {
            var site = await _siteRepository.GetAsync(request.SiteId);

            if (file == null)
            {
                return this.Error("请选择有效的文件上传");
            }

            var fileName = Path.GetFileName(file.FileName);

            if (!_pathManager.IsFileExtensionAllowed(site, PathUtils.GetExtension(fileName)))
            {
                return this.Error("文件格式不正确，请更换文件上传!");
            }

            var localDirectoryPath = await _pathManager.GetUploadDirectoryPathAsync(site, UploadType.File);
            var localFileName = PathUtils.GetUploadFileName(fileName, request.IsChangeFileName);
            var filePath = PathUtils.Combine(localDirectoryPath, localFileName);

            await _pathManager.UploadAsync(file, filePath);

            return new UploadResult
            {
                Name = localFileName,
                Path = filePath
            };
        }

        [HttpPost, Route(Route)]
        public async Task<ActionResult<List<SubmitResult>>> Submit([FromBody] SubmitRequest request)
        {
            var site = await _siteRepository.GetAsync(request.SiteId);
            if (site == null) return this.Error("无法确定内容对应的站点");

            var result = new List<SubmitResult>();
            foreach (var filePath in request.FilePaths)
            {
                if (string.IsNullOrEmpty(filePath)) continue;

                var fileName = PathUtils.GetFileName(filePath);

                var virtualUrl = await _pathManager.GetVirtualUrlByPhysicalPathAsync(site, filePath);
                var fileUrl = await _pathManager.ParseSiteUrlAsync(site, virtualUrl, true);

                if (request.IsLibrary)
                {
                    var materialFileName = PathUtils.GetMaterialFileName(fileName);
                    var virtualDirectoryPath = PathUtils.GetMaterialVirtualDirectoryPath(UploadType.Image);

                    var directoryPath = _pathManager.ParsePath(virtualDirectoryPath);
                    var materialFilePath = PathUtils.Combine(directoryPath, materialFileName);
                    DirectoryUtils.CreateDirectoryIfNotExists(materialFilePath);

                    FileUtils.CopyFile(filePath, materialFilePath, true);

                    var file = new MaterialFile
                    {
                        Title = fileName,
                        Url = PageUtils.Combine(virtualDirectoryPath, materialFileName)
                    };

                    await _materialFileRepository.InsertAsync(file);
                }


                result.Add(new SubmitResult
                {
                    FileUrl = fileUrl,
                    FileVirtualUrl = virtualUrl
                });
            }

            var options = TranslateUtils.JsonDeserialize(site.Get<string>(nameof(LayerFileUploadController)), new Options
            {
                IsChangeFileName = true,
                IsLibrary = true
            });

            options.IsChangeFileName = request.IsChangeFileName;
            options.IsLibrary = request.IsLibrary;
            site.Set(nameof(LayerFileUploadController), TranslateUtils.JsonSerialize(options));

            await _siteRepository.UpdateAsync(site);

            return result;
        }
    }
}
