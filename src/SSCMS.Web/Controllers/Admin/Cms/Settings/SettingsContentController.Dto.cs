﻿using SSCMS.Dto;

namespace SSCMS.Web.Controllers.Admin.Cms.Settings
{
    public partial class SettingsContentController
    {
        public class SubmitRequest : SiteRequest
        {
            public bool IsSaveImageInTextEditor { get; set; }
            public int PageSize { get; set; }
            public bool IsAutoPageInTextEditor { get; set; }
            public int AutoPageWordNum { get; set; }
            public bool IsContentTitleBreakLine { get; set; }
            public bool IsContentSubTitleBreakLine { get; set; }
            public bool IsAutoCheckKeywords { get; set; }
            public int CheckContentLevel { get; set; }
            public int CheckContentDefaultLevel { get; set; }
        }
    }
}