﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Datory;
using SSCMS.Core.Utils;
using SSCMS.Models;

namespace SSCMS.Core.Repositories
{
    public partial class TemplateRepository
    {
        private string GetListKey(int siteId)
        {
            return CacheUtils.GetListKey(_repository.TableName, siteId);
        }

        private string GetEntityKey(int templateId)
        {
            return CacheUtils.GetEntityKey(_repository.TableName, templateId);
        }

        public async Task<List<TemplateSummary>> GetSummariesAsync(int siteId)
        {
            return await _repository.GetAllAsync<TemplateSummary>(Q
                .Select(nameof(Template.Id), nameof(Template.TemplateName), nameof(Template.TemplateType), nameof(Template.DefaultTemplate))
                .Where(nameof(Template.SiteId), siteId)
                .OrderBy(nameof(Template.TemplateType), nameof(Template.RelatedFileName))
                .CachingGet(GetListKey(siteId))
            );
        }

        public async Task<Template> GetAsync(int templateId)
        {
            return await _repository.GetAsync(templateId, Q
                .CachingGet(GetEntityKey(templateId))
            );
        }
    }
}
