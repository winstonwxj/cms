﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datory;
using SSCMS.Core.Utils;
using SSCMS.Models;
using SSCMS.Repositories;
using SSCMS.Services;

namespace SSCMS.Core.Repositories
{
    public class WxMenuRepository : IWxMenuRepository
    {
        private readonly Repository<WxMenu> _repository;

        public WxMenuRepository(ISettingsManager settingsManager)
        {
            _repository = new Repository<WxMenu>(settingsManager.Database, settingsManager.Redis);
        }

        public IDatabase Database => _repository.Database;

        public string TableName => _repository.TableName;

        public List<TableColumn> TableColumns => _repository.TableColumns;

        private string GetCacheKey(int siteId) => CacheUtils.GetListKey(_repository.TableName, siteId);

        public async Task<int> InsertAsync(WxMenu menu)
        {
            return await _repository.InsertAsync(menu, Q
                .CachingRemove(GetCacheKey(menu.SiteId))
            );
        }

        public async Task UpdateAsync(WxMenu menu)
        {
            await _repository.UpdateAsync(menu, Q
                .CachingRemove(GetCacheKey(menu.SiteId))
            );
        }

        public async Task DeleteAsync(int siteId, int menuId)
        {
            await _repository.DeleteAsync(menuId, Q
                .CachingRemove(GetCacheKey(siteId))
            );
        }

        public async Task DeleteAllAsync(int siteId)
        {
            await _repository.DeleteAsync(Q
                .Where(nameof(WxMenu.SiteId), siteId)
                .CachingRemove(GetCacheKey(siteId))
            );
        }

        public async Task<List<WxMenu>> GetMenusAsync(int siteId)
        {
            var list = await _repository.GetAllAsync(Q
                .Where(nameof(WxReplyKeyword.SiteId), siteId)
                .CachingGet(GetCacheKey(siteId))
            );

            return list.OrderBy(menu => menu.Taxis == 0 ? int.MaxValue : menu.Taxis).ToList();
        }

        public async Task<WxMenu> GetAsync(int siteId, int id)
        {
            var infoList = await _repository.GetAllAsync(Q
                .CachingGet(GetCacheKey(siteId))
            );
            return infoList.FirstOrDefault(x => x.Id == id);
        }
    }
}
