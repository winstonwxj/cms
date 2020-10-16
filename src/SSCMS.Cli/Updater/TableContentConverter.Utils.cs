﻿using System.Collections.Generic;
using Datory;
using SSCMS.Models;
using SSCMS.Utils;

namespace SSCMS.Cli.Updater
{
    public partial class TableContentConverter
    {
        public ConvertInfo GetSplitConverter()
        {
            return new ConvertInfo
            {
                NewColumns = GetNewColumns(null),
                ConvertKeyDict = ConvertKeyDict,
                ConvertValueDict = ConvertValueDict
            };
        }

        public ConvertInfo GetConverter(string oldTableName, List<TableColumn> oldColumns)
        {
            return new ConvertInfo
            {
                NewTableName = oldTableName,
                NewColumns = GetNewColumns(oldColumns),
                ConvertKeyDict = ConvertKeyDict,
                ConvertValueDict = ConvertValueDict,
                Process = Process
            };
        }

        private List<TableColumn> GetNewColumns(List<TableColumn> oldColumns)
        {
            var columns = new List<TableColumn>();
            var repository =
                new Repository<Content>(_settingsManager.Database);
            columns.AddRange(repository.TableColumns);

            if (oldColumns != null && oldColumns.Count > 0)
            {
                foreach (var tableColumnInfo in oldColumns)
                {
                    if (StringUtils.EqualsIgnoreCase(tableColumnInfo.AttributeName, nameof(NodeId)))
                    {
                        tableColumnInfo.AttributeName = nameof(SSCMS.Models.Content.ChannelId);
                    }
                    else if (StringUtils.EqualsIgnoreCase(tableColumnInfo.AttributeName, nameof(PublishmentSystemId)))
                    {
                        tableColumnInfo.AttributeName = nameof(SSCMS.Models.Content.SiteId);
                    }
                    else if (StringUtils.EqualsIgnoreCase(tableColumnInfo.AttributeName, nameof(ContentGroupNameCollection)))
                    {
                        tableColumnInfo.AttributeName = nameof(SSCMS.Models.Content.GroupNames);
                    }
                    else if (StringUtils.EqualsIgnoreCase(tableColumnInfo.AttributeName, nameof(GroupNameCollection)))
                    {
                        tableColumnInfo.AttributeName = nameof(SSCMS.Models.Content.GroupNames);
                    }

                    if (!columns.Exists(c => StringUtils.EqualsIgnoreCase(c.AttributeName, tableColumnInfo.AttributeName)))
                    {
                        columns.Add(tableColumnInfo);
                    }
                }
            }

            return columns;
        }

        private static readonly Dictionary<string, string> ConvertKeyDict =
            new Dictionary<string, string>
            {
                {nameof(SSCMS.Models.Content.ChannelId), nameof(NodeId)},
                {nameof(SSCMS.Models.Content.SiteId), nameof(PublishmentSystemId)},
                {nameof(SSCMS.Models.Content.GroupNames), nameof(ContentGroupNameCollection)},
                {nameof(SSCMS.Models.Content.GroupNames), nameof(GroupNameCollection)}
            };

        private static readonly Dictionary<string, string> ConvertValueDict = null;

        private static Dictionary<string, object> Process(Dictionary<string, object> row)
        {
            if (row.TryGetValue(nameof(Content), out var contentObj))
            {
                var content = contentObj.ToString();
                content = content.Replace("@upload", "@/upload");
                row[nameof(SSCMS.Models.Content.Body)] = content;
            }
            if (row.TryGetValue(nameof(SettingsXml), out contentObj))
            {
                var content = contentObj.ToString();
                content = content.Replace("@upload", "@/upload");
                row["ExtendValues"] = content;
            }

            return row;
        }
    }
}
