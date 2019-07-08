﻿using System;
using System.Collections.Generic;
using SS.CMS.Data;
using SS.CMS.Models;

namespace SS.CMS.Core.Models.Attributes
{
    public static class ContentAttribute
    {
        public const string Id = nameof(Entity.Id);
        public const string Guid = nameof(Entity.Guid);
        public const string CreatedDate = nameof(Entity.CreatedDate);
        public const string LastModifiedDate = nameof(Entity.LastModifiedDate);
        public const string ChannelId = nameof(ContentInfo.ChannelId);
        public const string SiteId = nameof(ContentInfo.SiteId);
        public const string UserId = nameof(ContentInfo.UserId);
        public const string LastModifiedUserId = nameof(ContentInfo.LastModifiedUserId);
        public const string Taxis = nameof(ContentInfo.Taxis);
        public const string GroupNameCollection = nameof(ContentInfo.GroupNameCollection);
        public const string Tags = nameof(ContentInfo.Tags);
        public const string SourceId = nameof(ContentInfo.SourceId);
        public const string ReferenceId = nameof(ContentInfo.ReferenceId);
        public const string IsChecked = nameof(ContentInfo.IsChecked);
        public const string CheckedLevel = nameof(ContentInfo.CheckedLevel);
        public const string Hits = nameof(ContentInfo.Hits);
        public const string HitsByDay = nameof(ContentInfo.HitsByDay);
        public const string HitsByWeek = nameof(ContentInfo.HitsByWeek);
        public const string HitsByMonth = nameof(ContentInfo.HitsByMonth);
        public const string LastHitsDate = nameof(ContentInfo.LastHitsDate);
        public const string Downloads = nameof(ContentInfo.Downloads);
        public const string ExtendValues = nameof(ContentInfo.ExtendValues);
        public const string Title = nameof(ContentInfo.Title);
        public const string IsTop = nameof(ContentInfo.IsTop);
        public const string IsRecommend = nameof(ContentInfo.IsRecommend);
        public const string IsHot = nameof(ContentInfo.IsHot);
        public const string IsColor = nameof(ContentInfo.IsColor);
        public const string LinkUrl = nameof(ContentInfo.LinkUrl);
        public const string AddDate = nameof(ContentInfo.AddDate);
        public const string Content = nameof(ContentInfo.Content);
        public const string SubTitle = nameof(ContentInfo.SubTitle);
        public const string ImageUrl = nameof(ContentInfo.ImageUrl);
        public const string VideoUrl = nameof(ContentInfo.VideoUrl);
        public const string FileUrl = nameof(ContentInfo.FileUrl);
        public const string Author = nameof(ContentInfo.Author);
        public const string Source = nameof(ContentInfo.Source);
        public const string Summary = nameof(ContentInfo.Summary);

        public static string GetFormatStringAttributeName(string attributeName)
        {
            return attributeName + "FormatString";
        }

        public static string GetExtendAttributeName(string attributeName)
        {
            return attributeName + "_Extend";
        }

        // 计算字段
        public const string Sequence = nameof(Sequence);                            //序号
        public const string PageContent = nameof(PageContent);
        public const string NavigationUrl = nameof(NavigationUrl);
        public const string CheckState = nameof(CheckState);

        // 附加字段
        public const string CheckUserId = nameof(CheckUserId);                  //审核者
        public const string CheckDate = nameof(CheckDate);                          //审核时间
        public const string CheckReasons = nameof(CheckReasons);                    //审核原因
        public const string TranslateContentType = nameof(TranslateContentType);    //转移内容类型

        public static readonly Lazy<List<string>> AllAttributes = new Lazy<List<string>>(() => new List<string>
        {
            Id,
            ChannelId,
            SiteId,
            UserId,
            LastModifiedUserId,
            Taxis,
            GroupNameCollection,
            Tags,
            SourceId,
            ReferenceId,
            IsChecked,
            CheckedLevel,
            Hits,
            HitsByDay,
            HitsByWeek,
            HitsByMonth,
            LastHitsDate,
            Downloads,
            ExtendValues,
            Title,
            IsTop,
            IsRecommend,
            IsHot,
            IsColor,
            LinkUrl,
            AddDate
        });

        public static readonly Lazy<List<string>> MetadataAttributes = new Lazy<List<string>>(() => new List<string>
        {
            Id,
            ChannelId,
            SiteId,
            UserId,
            LastModifiedUserId,
            Taxis,
            GroupNameCollection,
            Tags,
            SourceId,
            ReferenceId,
            IsChecked,
            CheckedLevel,
            Hits,
            HitsByDay,
            HitsByWeek,
            HitsByMonth,
            LastHitsDate,
            Downloads,
            ExtendValues,
            IsTop,
            IsRecommend,
            IsHot,
            IsColor,
            AddDate,
            LinkUrl
        });

        public static readonly Lazy<List<string>> CalculateAttributes = new Lazy<List<string>>(() => new List<string>
        {
            Sequence,
            UserId,
            LastModifiedUserId,
            SourceId
        });

        public static readonly Lazy<List<string>> DropAttributes = new Lazy<List<string>>(() => new List<string>
        {
            "WritingUserName",
            "ConsumePoint",
            "Comments",
            "Reply",
            "CheckTaskDate",
            "UnCheckTaskDate"
        });
    }
}
