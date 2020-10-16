﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Datory;
using SSCMS.Core.Utils;
using SSCMS.Enums;
using SSCMS.Models;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Core.Repositories
{
    public partial class ChannelRepository : IChannelRepository
    {
        private readonly Repository<Channel> _repository;
        private readonly ISettingsManager _settingsManager;
        private readonly ITemplateRepository _templateRepository;

        public ChannelRepository(ISettingsManager settingsManager, ITemplateRepository templateRepository)
        {
            _repository = new Repository<Channel>(settingsManager.Database, settingsManager.Redis);
            _settingsManager = settingsManager;
            _templateRepository = templateRepository;
        }

        public IDatabase Database => _repository.Database;

        public string TableName => _repository.TableName;

        public List<TableColumn> TableColumns => _repository.TableColumns;

        public async Task InsertChannelAsync(Channel parentChannel, Channel channel)
        {
            if (parentChannel != null)
            {
                channel.SiteId = parentChannel.SiteId;
                if (string.IsNullOrEmpty(parentChannel.ParentsPath))
                {
                    channel.ParentsPath = parentChannel.Id.ToString();
                }
                else
                {
                    channel.ParentsPath = parentChannel.ParentsPath + "," + parentChannel.Id;
                }
                channel.ParentsCount = parentChannel.ParentsCount + 1;

                var maxTaxis = await GetMaxTaxisAsync(channel.SiteId, channel.ParentId);
                if (maxTaxis == 0)
                {
                    maxTaxis = parentChannel.Taxis;
                }
                channel.Taxis = maxTaxis + 1;
            }
            else
            {
                channel.Taxis = 1;
            }

            channel.ChildrenCount = 0;

            if (channel.SiteId != 0)
            {
                await _repository.IncrementAsync(nameof(Channel.Taxis), Q
                    .Where(nameof(Channel.Taxis), ">=", channel.Taxis)
                    .Where(nameof(Channel.SiteId), channel.SiteId)
                );
            }

            channel.Id = await _repository.InsertAsync(channel, Q
                .CachingRemove(GetListKey(channel.SiteId))
            );

            if (!string.IsNullOrEmpty(channel.ParentsPath))
            {
                await _repository.IncrementAsync(nameof(Channel.ChildrenCount), Q
                    .WhereIn(nameof(Channel.Id), ListUtils.GetIntList(channel.ParentsPath))
                );
            }
        }

        public async Task<int> InsertAsync(int siteId, int parentId, string channelName, string indexName, string contentModelPluginId, int channelTemplateId, int contentTemplateId)
        {
            if (siteId > 0 && parentId == 0) return 0;

            var defaultChannelTemplateEntity = await _templateRepository.GetDefaultTemplateAsync(siteId, TemplateType.ChannelTemplate);
            var defaultContentTemplateEntity = await _templateRepository.GetDefaultTemplateAsync(siteId, TemplateType.ContentTemplate);

            var channelEntity = new Channel
            {
                ParentId = parentId,
                SiteId = siteId,
                ChannelName = channelName,
                IndexName = indexName,
                ContentModelPluginId = contentModelPluginId,
                AddDate = DateTime.Now,
                ChannelTemplateId = channelTemplateId > 0 ? channelTemplateId : defaultChannelTemplateEntity.Id,
                ContentTemplateId = contentTemplateId > 0 ? contentTemplateId : defaultContentTemplateEntity.Id
            };

            var parentChannel = await GetAsync(parentId);
            if (parentChannel != null)
            {
                if (parentChannel.DefaultTaxisType != TaxisType.OrderByTaxisDesc)
                {
                    channelEntity.DefaultTaxisType = parentChannel.DefaultTaxisType;
                }
            }

            await InsertChannelAsync(parentChannel, channelEntity);

            return channelEntity.Id;

        }

        public async Task<int> InsertAsync(Channel channel)
        {
            if (channel.SiteId > 0 && channel.ParentId == 0) return 0;

            var parentChannel = await GetAsync(channel.ParentId);

            await InsertChannelAsync(parentChannel, channel);

            return channel.Id;
        }

        public async Task UpdateAsync(Channel channel)
        {
            if (channel.ParentId == channel.Id)
            {
                channel.ParentId = 0;
            }
            await _repository.UpdateAsync(channel, Q
                .CachingRemove(GetListKey(channel.SiteId))
                .CachingRemove(GetEntityKey(channel.Id))
            );
        }

        public async Task UpdateChannelTemplateIdAsync(Channel channel)
        {
            await _repository.UpdateAsync(Q
                .Set(nameof(Channel.ChannelTemplateId), channel.ChannelTemplateId)
                .Where(nameof(Channel.Id), channel.Id)
            );
        }

        public async Task UpdateContentTemplateIdAsync(Channel channel)
        {
            await _repository.UpdateAsync(Q
                .Set(nameof(Channel.ContentTemplateId), channel.ContentTemplateId)
                .Where(nameof(Channel.Id), channel.Id)
            );
        }

        public async Task DeleteAsync(Site site, int channelId, int adminId)
        {
            var channelEntity = await GetAsync(channelId);
            if (channelEntity == null) return;

            var idList = new List<int>();
            if (channelEntity.ChildrenCount > 0)
            {
                idList = await GetChannelIdsAsync(site.Id, channelId, ScopeType.Descendant);
            }
            idList.Add(channelId);

            //_contentRepository.DeleteContentsByDeletedChannelIdList(trans, site, idList);

            var deletedNum = await _repository.DeleteAsync(Q
                .Where(nameof(Channel.SiteId), site.Id)
                .WhereIn(nameof(Channel.Id), idList)
                .CachingRemove(GetListKey(site.Id))
            );

            if (channelEntity.ParentId != 0)
            {
                await _repository.DecrementAsync(nameof(Channel.Taxis), Q
                    .Where(nameof(Channel.SiteId), channelEntity.SiteId)
                    .Where(nameof(Channel.Taxis), ">", channelEntity.Taxis)
                    .CachingRemove(GetListKey(site.Id))
                , deletedNum);
            }

            if (!string.IsNullOrEmpty(channelEntity.ParentsPath))
            {
                await _repository.DecrementAsync(nameof(Channel.ChildrenCount), Q
                        .WhereIn(nameof(Channel.Id), ListUtils.GetIntList(channelEntity.ParentsPath))
                        .CachingRemove(GetListKey(site.Id))
                    , deletedNum);
            }
        }

        public async Task DeleteAllAsync(int siteId)
        {
            await _repository.DeleteAsync(Q
                .Where(nameof(Channel.SiteId), siteId)
                .OrWhere(nameof(Channel.Id), siteId)
                .CachingRemove(GetListKey(siteId))
            );
        }

        public async Task<bool> IsFilePathExistsAsync(int siteId, string filePath)
        {
            return await _repository.ExistsAsync(Q
                .Where(nameof(Channel.SiteId), siteId)
                .Where(nameof(Channel.FilePath), filePath)
            );
        }

        private static string GetGroupWhereString(DatabaseType databaseType, string group, string groupNot)
        {
            var whereStringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(group))
            {
                group = group.Trim().Trim(',');
                var groupArr = group.Split(',');
                if (groupArr.Length > 0)
                {
                    whereStringBuilder.Append(" AND (");
                    foreach (var theGroup in groupArr)
                    {
                        var trimGroup = theGroup.Trim();

                        whereStringBuilder.Append(
                                $" (siteserver_Channel.GroupNames = '{trimGroup}' OR {SqlUtils.GetInStr(databaseType, "siteserver_Channel.GroupNames", trimGroup + ",")} OR {SqlUtils.GetInStr(databaseType, "siteserver_Channel.GroupNames", "," + trimGroup + ",")} OR {SqlUtils.GetInStr(databaseType, "siteserver_Channel.GroupNames", "," + trimGroup)}) OR ");
                    }
                    if (groupArr.Length > 0)
                    {
                        whereStringBuilder.Length -= 3;
                    }
                    whereStringBuilder.Append(") ");
                }
            }

            if (!string.IsNullOrEmpty(groupNot))
            {
                groupNot = groupNot.Trim().Trim(',');
                var groupNotArr = groupNot.Split(',');
                if (groupNotArr.Length > 0)
                {
                    whereStringBuilder.Append(" AND (");
                    foreach (var theGroupNot in groupNotArr)
                    {
                        var trimGroupNot = AttackUtils.FilterSql(theGroupNot.Trim());
                        //whereStringBuilder.Append(
                        //    $" (siteserver_Channel.GroupNames <> '{trimGroupNot}' AND CHARINDEX('{trimGroupNot},',siteserver_Channel.GroupNames) = 0 AND CHARINDEX(',{trimGroupNot},',siteserver_Channel.GroupNames) = 0 AND CHARINDEX(',{trimGroupNot}',siteserver_Channel.GroupNames) = 0) AND ");

                        whereStringBuilder.Append(
                                $" (siteserver_Channel.GroupNames <> '{trimGroupNot}' AND {SqlUtils.GetNotInStr(databaseType, "siteserver_Channel.GroupNames", trimGroupNot + ",")} AND {SqlUtils.GetNotInStr(databaseType, "siteserver_Channel.GroupNames", "," + trimGroupNot + ",")} AND {SqlUtils.GetNotInStr(databaseType, "siteserver_Channel.GroupNames", "," + trimGroupNot)}) AND ");
                    }
                    if (groupNotArr.Length > 0)
                    {
                        whereStringBuilder.Length -= 4;
                    }
                    whereStringBuilder.Append(") ");
                }
            }
            return whereStringBuilder.ToString();
        }

        public string GetWhereString(string group, string groupNot, bool isImageExists, bool isImage)
        {
            var whereStringBuilder = new StringBuilder();
            if (isImageExists)
            {
                whereStringBuilder.Append(isImage
                    ? " AND siteserver_Channel.ImageUrl <> '' "
                    : " AND siteserver_Channel.ImageUrl = '' ");
            }

            whereStringBuilder.Append(GetGroupWhereString(Database.DatabaseType, group, groupNot));

            return whereStringBuilder.ToString();
        }

        public async Task<List<int>> GetChannelIdsByTotalNumAsync(List<int> channelIdList, int totalNum, string orderByString, string whereString)
        {
            if (channelIdList == null || channelIdList.Count == 0)
            {
                return channelIdList;
            }

            string sqlString;
            if (totalNum > 0)
            {
                var where =
                    $"WHERE {SqlUtils.GetSqlColumnInList("Id", channelIdList)} {whereString})";
                sqlString = SqlUtils.ToTopSqlString(Database.DatabaseType, TableName, "Id",
                    where,
                    orderByString,
                    totalNum);

                //return await _repository.GetAllAsync<int>(Q
                //    .Select(nameof(Channel.Id))
                //    .Limit(totalNum)
                //);
            }
            else
            {
                sqlString = $@"SELECT Id
FROM siteserver_Channel 
WHERE {SqlUtils.GetSqlColumnInList("Id", channelIdList)} {whereString} {orderByString}
";
            }

            var list = new List<int>();

            using (var connection = _repository.Database.GetConnection())
            {
                using var rdr = await connection.ExecuteReaderAsync(sqlString);
                while (rdr.Read())
                {
                    if (!rdr.IsDBNull(0))
                    {
                        list.Add(rdr.GetInt32(0));
                    }
                }
                rdr.Close();
            }

            return list;
        }

        public async Task<List<string>> GetAllFilePathBySiteIdAsync(int siteId)
        {
            return await _repository.GetAllAsync<string>(Q
                .Select(nameof(Channel.FilePath))
                .WhereNotNull(nameof(Channel.FilePath))
                .WhereNot(nameof(Channel.FilePath), string.Empty)
                .Where(nameof(Channel.SiteId), siteId)
            );
        }

        public int GetTemplateUseCount(int siteId, int templateId, TemplateType templateType, bool isDefault, List<Channel> channels)
        {
            if (templateType == TemplateType.IndexPageTemplate)
            {
                if (isDefault)
                {
                    return 1;
                }
            }
            else if (templateType == TemplateType.FileTemplate)
            {
                return 1;
            }
            else if (templateType == TemplateType.ChannelTemplate)
            {
                foreach (var channel in channels)
                {
                    var count = 0;
                    if (isDefault)
                    {
                        if (channel.ChannelTemplateId == templateId || channel.ChannelTemplateId == 0)
                        {
                            count++;
                        }
                    }
                    else
                    {
                        if (channel.ChannelTemplateId == templateId)
                        {
                            count++;
                        }
                    }

                    return count;
                }
            }
            else if (templateType == TemplateType.ContentTemplate)
            {
                foreach (var channel in channels)
                {
                    var count = 0;
                    if (isDefault)
                    {
                        if (channel.ContentTemplateId == templateId || channel.ContentTemplateId == 0)
                        {
                            count++;
                        }
                    }
                    else
                    {
                        if (channel.ContentTemplateId == templateId)
                        {
                            count++;
                        }
                    }

                    return count;
                }
            }

            return 0;
        }

        public async Task<List<int>> GetChannelIdsAsync(Template template)
        {
            if (template.TemplateType != TemplateType.ChannelTemplate &&
                template.TemplateType != TemplateType.ContentTemplate)
            {
                return new List<int>();
            }

            if (template.TemplateType == TemplateType.ChannelTemplate)
            {
                if (template.DefaultTemplate)
                {
                    return await _repository.GetAllAsync<int>(Q
                        .Select(nameof(Channel.Id))
                        .Where(nameof(Channel.SiteId), template.SiteId)
                        .Where(q => q
                            .Where(nameof(Channel.ChannelTemplateId), template.Id)
                            .OrWhere(nameof(Channel.ChannelTemplateId), 0)
                        )
                    );
                }

                return await _repository.GetAllAsync<int>(Q
                    .Select(nameof(Channel.Id))
                    .Where(nameof(Channel.SiteId), template.SiteId)
                    .Where(nameof(Channel.ChannelTemplateId), template.Id)
                );
            }
            
            if (template.DefaultTemplate)
            {
                return await _repository.GetAllAsync<int>(Q
                    .Select(nameof(Channel.Id))
                    .Where(nameof(Channel.SiteId), template.SiteId)
                    .Where(q => q
                        .Where(nameof(Channel.ContentTemplateId), template.Id)
                        .OrWhere(nameof(Channel.ContentTemplateId), 0)
                    )
                );
            }

            return await _repository.GetAllAsync<int>(Q
                .Select(nameof(Channel.Id))
                .Where(nameof(Channel.SiteId), template.SiteId)
                .Where(nameof(Channel.ContentTemplateId), template.Id)
            );
        }

        public List<int> GetChannelIdsByTemplateId(bool isChannelTemplate, int templateId, List<Channel> channels)
        {
            return isChannelTemplate
                ? channels.Where(x => x.ChannelTemplateId == templateId).Select(x => x.Id).ToList()
                : channels.Where(x => x.ContentTemplateId == templateId).Select(x => x.Id).ToList();
        }
    }
}
