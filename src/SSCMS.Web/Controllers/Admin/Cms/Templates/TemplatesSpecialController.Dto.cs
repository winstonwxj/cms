﻿using System.Collections.Generic;
using SSCMS.Dto;
using SSCMS.Models;

namespace SSCMS.Web.Controllers.Admin.Cms.Templates
{
    public partial class TemplatesSpecialController
    {
        public class ListResult
        {
            public List<Special> Specials { get; set; }
            public string SiteUrl { get; set; }
        }

        public class SpecialIdRequest : SiteRequest
        {
            public int SpecialId { get; set; }
        }

        public class DeleteResult
        {
            public List<Special> Specials { get; set; }
        }

        public class GetSpecialResult
        {
            public Special Special { get; set; }
            public string Guid { get; set; }
        }

        public class UploadRequest : SiteRequest
        {
            public string Guid { get; set; }
        }

        public class SubmitRequest : SiteRequest
        {
            public int Id { get; set; }
            public string Guid { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }
            public IEnumerable<string> FileNames { get; set; }
            public bool IsEditOnly { get; set; }
            public bool IsUploadOnly { get; set; }
        }
    }
}