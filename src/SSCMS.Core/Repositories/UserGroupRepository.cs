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
    public partial class UserGroupRepository : IUserGroupRepository
    {
        private readonly Repository<UserGroup> _repository;
        private readonly IConfigRepository _configRepository;

        public UserGroupRepository(ISettingsManager settingsManager, IConfigRepository configRepository)
        {
            _repository = new Repository<UserGroup>(settingsManager.Database, settingsManager.Redis);
            _configRepository = configRepository;
        }

        public IDatabase Database => _repository.Database;

        public string TableName => _repository.TableName;

        public List<TableColumn> TableColumns => _repository.TableColumns;

        private string CacheKey => CacheUtils.GetListKey(_repository.TableName);

        public async Task<int> InsertAsync(UserGroup group)
        {
            return await _repository.InsertAsync(group, Q.CachingRemove(CacheKey));
        }

        public async Task UpdateAsync(UserGroup group)
        {
            await _repository.UpdateAsync(group, Q.CachingRemove(CacheKey));
        }

        public async Task ClearCache()
        {
            await _repository.RemoveCacheAsync(CacheKey);
        }

        public async Task DeleteAsync(int groupId)
        {
            await _repository.DeleteAsync(groupId, Q.CachingRemove(CacheKey));
        }

        public async Task<List<UserGroup>> GetUserGroupsAsync()
        {
            var config = await _configRepository.GetAsync();

            var list = (await _repository.GetAllAsync(Q
                .OrderBy(nameof(UserGroup.Id))
                .CachingGet(CacheKey)
            )).ToList();

            list.Insert(0, new UserGroup
            {
                Id = 0,
                GroupName = "默认用户组",
                AdminName = config.UserDefaultGroupAdminName
            });

            return list;
        }
    }
}
