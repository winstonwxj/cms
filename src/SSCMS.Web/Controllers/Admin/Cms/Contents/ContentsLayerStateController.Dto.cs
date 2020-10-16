﻿using System.Collections.Generic;
using SSCMS.Dto;
using SSCMS.Models;

namespace SSCMS.Web.Controllers.Admin.Cms.Contents
{
    public partial class ContentsLayerStateController
    {
        public class GetRequest : ChannelRequest
        {
            public int ContentId { get; set; }
        }

        public class GetResult
        {
            public List<ContentCheck> ContentChecks { get; set; }
            public Content Content { get; set; }
            public string State { get; set; }
        }
    }
}