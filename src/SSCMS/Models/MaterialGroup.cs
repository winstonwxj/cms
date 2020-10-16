﻿using Datory;
using Datory.Annotations;
using SSCMS.Enums;

namespace SSCMS.Models
{
    [DataTable("siteserver_MaterialGroup")]
    public class MaterialGroup : Entity
    {
        [DataColumn]
        public MaterialType LibraryType { get; set; }

        [DataColumn]
        public string GroupName { get; set; }
    }
}
