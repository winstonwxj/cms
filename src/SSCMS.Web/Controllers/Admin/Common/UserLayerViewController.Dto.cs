﻿using SSCMS.Models;

namespace SSCMS.Web.Controllers.Admin.Common
{
    public partial class UserLayerViewController
    {
        public class GetRequest
        {
            public int UserId { get; set; }
            public string UserName { get; set; }
        }

        public class GetResult
        {
            public User User { get; set; }
            public string GroupName { get; set; }
        }
    }
}
