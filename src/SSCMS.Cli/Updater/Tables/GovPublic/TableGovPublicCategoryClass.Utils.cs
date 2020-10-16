﻿using System.Collections.Generic;
using Datory;

namespace SSCMS.Cli.Updater.Tables.GovPublic
{
    public partial class TableGovPublicCategoryClass
    {
        public const string OldTableName = "wcm_GovPublicCategoryClass";

        public static ConvertInfo Converter => new ConvertInfo
        {
            NewTableName = NewTableName,
            NewColumns = NewColumns,
            ConvertKeyDict = ConvertKeyDict,
            ConvertValueDict = ConvertValueDict
        };

        private static readonly string NewTableName = "ss_govpublic_category_class";

        private static readonly List<TableColumn> NewColumns = new List<TableColumn>
        {
            new TableColumn
            {
                AttributeName = "Id",
                DataType = DataType.Integer,
                IsPrimaryKey = true,
                IsIdentity = true
            },
            new TableColumn
            {
                AttributeName = "SiteId",
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = "ClassCode",
                DataType = DataType.VarChar,
                DataLength = 200
            },
            new TableColumn
            {
                AttributeName = "ClassName",
                DataType = DataType.VarChar,
                DataLength = 200
            },
            new TableColumn
            {
                AttributeName = "IsSystem",
                DataType = DataType.Boolean
            },
            new TableColumn
            {
                AttributeName = "IsEnabled",
                DataType = DataType.Boolean
            },
            new TableColumn
            {
                AttributeName = "ContentAttributeName",
                DataType = DataType.VarChar,
                DataLength = 200
            },
            new TableColumn
            {
                AttributeName = "Taxis",
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = "Description",
                DataType = DataType.VarChar,
                DataLength = 200
            }
        };

        private static readonly Dictionary<string, string> ConvertKeyDict =
            new Dictionary<string, string>
            {
                {"SiteId", nameof(PublishmentSystemId)}
            };

        private static readonly Dictionary<string, string> ConvertValueDict = null;
    }
}
