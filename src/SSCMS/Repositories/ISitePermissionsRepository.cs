﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Datory;
using SSCMS.Models;

namespace SSCMS.Repositories
{
    public interface ISitePermissionsRepository : IRepository
    {
        Task InsertAsync(SitePermissions permissions);

        Task DeleteAsync(string roleName);

        Task<List<SitePermissions>> GetAllAsync(string roleName);

        Task<SitePermissions> GetAsync(string roleName, int siteId);

        Task<Dictionary<int, List<string>>> GetSitePermissionDictionaryAsync(IEnumerable<string> roles);

        Task<Dictionary<string, List<string>>> GetChannelPermissionDictionaryAsync(IList<string> roles);

        Task<Dictionary<string, List<string>>> GetContentPermissionDictionaryAsync(IList<string> roles);

        Task<List<string>> GetChannelPermissionListIgnoreChannelIdAsync(IList<string> roles);
    }
}
