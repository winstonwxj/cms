﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SSCMS.Configuration;
using SSCMS.Models;
using SSCMS.Repositories;
using SSCMS.Services;

namespace SSCMS.Web.Controllers.Admin.Wx
{
    [OpenApiIgnore]
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class LayerVideoController : ControllerBase
    {
        private const string Route = "wx/layerVideo";

        private readonly IAuthManager _authManager;
        private readonly IMaterialGroupRepository _materialGroupRepository;
        private readonly IMaterialVideoRepository _materialVideoRepository;

        public LayerVideoController(IAuthManager authManager, IMaterialGroupRepository materialGroupRepository, IMaterialVideoRepository materialVideoRepository)
        {
            _authManager = authManager;
            _materialGroupRepository = materialGroupRepository;
            _materialVideoRepository = materialVideoRepository;
        }

        public class QueryRequest
        {
            public int SiteId { get; set; }
            public string Keyword { get; set; }
            public int GroupId { get; set; }
            public int Page { get; set; }
            public int PerPage { get; set; }
        }

        public class QueryResult
        {
            public IEnumerable<MaterialGroup> Groups { get; set; }
            public int Count { get; set; }
            public IEnumerable<MaterialVideo> Videos { get; set; }
        }
    }
}
