﻿using System.Threading.Tasks;
using Datory;
using SSCMS.Models;
using SSCMS.Services;

namespace SSCMS.Repositories
{
    public partial interface ITemplateRepository : IRepository
    {
        Task<int> InsertAsync(IPathManager pathManager, Site site, Template template, string templateContent, int adminId);

        Task UpdateAsync(IPathManager pathManager, Site site, Template template, string templateContent, int adminId);

        Task SetDefaultAsync(int templateId);

        Task DeleteAsync(IPathManager pathManager, Site site, int templateId);

        Task CreateDefaultTemplateAsync(IPathManager pathManager, Site site, int adminId);
    }
}