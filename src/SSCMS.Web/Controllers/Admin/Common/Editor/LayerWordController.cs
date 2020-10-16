﻿using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SSCMS.Configuration;
using SSCMS.Core.Utils.Office;
using SSCMS.Dto;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Web.Controllers.Admin.Common.Editor
{
    [OpenApiIgnore]
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class LayerWordController : ControllerBase
    {
        private const string Route = "common/editor/layerWord";
        private const string RouteUpload = "common/editor/layerWord/actions/upload";

        private readonly IPathManager _pathManager;
        private readonly ISiteRepository _siteRepository;

        public LayerWordController(IPathManager pathManager, ISiteRepository siteRepository)
        {
            _pathManager = pathManager;
            _siteRepository = siteRepository;
        }

        [RequestSizeLimit(long.MaxValue)]
        [HttpPost, Route(RouteUpload)]
        public async Task<ActionResult<NameTitle>> Upload([FromQuery] SiteRequest request, [FromForm] IFormFile file)
        {
            if (file == null)
            {
                return this.Error("请选择有效的文件上传");
            }

            var title = PathUtils.GetFileNameWithoutExtension(file.FileName);
            var fileName = PathUtils.GetUploadFileName(file.FileName, true);
            var extendName = PathUtils.GetExtension(fileName);

            if (!FileUtils.IsWord(extendName))
            {
                return this.Error("文件只能是 Word 格式，请选择有效的文件上传!");
            }

            var filePath = _pathManager.GetTemporaryFilesPath(fileName);
            await _pathManager.UploadAsync(file, filePath);

            return new NameTitle
            {
                FileName = fileName,
                Title = title
            };
        }

        [HttpPost, Route(Route)]
        public async Task<ActionResult<StringResult>> Submit([FromBody] SubmitRequest request)
        {
            var site = await _siteRepository.GetAsync(request.SiteId);
            if (site == null) return this.Error("无法确定内容对应的站点");

            var builder = new StringBuilder();
            foreach (var file in request.Files)
            {
                if (string.IsNullOrEmpty(file.FileName) || string.IsNullOrEmpty(file.Title)) continue;

                var filePath = _pathManager.GetTemporaryFilesPath(file.FileName);
                var (_, _, wordContent) = await WordManager.GetWordAsync(_pathManager, site, false, request.IsClearFormat, request.IsFirstLineIndent, request.IsClearFontSize, request.IsClearFontFamily, request.IsClearImages, filePath, file.Title);
                wordContent = await _pathManager.DecodeTextEditorAsync(site, wordContent, true);
                builder.Append(wordContent);
                FileUtils.DeleteFileIfExists(filePath);
            }

            return new StringResult
            {
                Value = builder.ToString()
            };
        }
    }
}
