﻿using Datory;
using SSCMS.Configuration;
using SSCMS.Utils;

namespace SSCMS.Core.Utils
{
    public static class InstallUtils
    {
        public static void SaveSettings(string contentRootPath, bool isProtectData, bool isDisablePlugins, string securityKey, string databaseType, string databaseConnectionString, string redisConnectionString, string adminRestrictionHost, string[] adminRestrictionAllowList, string[] adminRestrictionBlockList)
        {
            var path = PathUtils.Combine(contentRootPath, Constants.ConfigFileName);

            if (adminRestrictionAllowList == null)
            {
                adminRestrictionAllowList = new string[] { };
            }
            if (adminRestrictionBlockList == null)
            {
                adminRestrictionBlockList = new string[] { };
            }

            var json = $@"
{{
  ""IsProtectData"": {StringUtils.ToLower(isProtectData.ToString())},
  ""IsDisablePlugins"": {StringUtils.ToLower(isDisablePlugins.ToString())},
  ""SecurityKey"": ""{securityKey}"",
  ""Database"": {{
    ""Type"": ""{databaseType}"",
    ""ConnectionString"": ""{StringUtils.ToJsonString(databaseConnectionString)}""
  }},
  ""Redis"": {{
    ""ConnectionString"": ""{redisConnectionString}""
  }},
  ""AdminRestriction"": {{
    ""Host"": ""{adminRestrictionHost}"",
    ""AllowList"": {TranslateUtils.JsonSerialize(adminRestrictionAllowList)},
    ""BlockList"": {TranslateUtils.JsonSerialize(adminRestrictionBlockList)}
  }}
}}";

            FileUtils.WriteText(path, json.Trim());

            var webConfigPath = PathUtils.Combine(contentRootPath, "Web.config");
            if (FileUtils.IsFileExists(webConfigPath))
            {
                var webConfigContent = FileUtils.ReadText(webConfigPath);
                FileUtils.WriteText(webConfigPath, webConfigContent);
            }
        }

        public static void Init(string contentRootPath)
        {
            var filePath = PathUtils.Combine(contentRootPath, Constants.ConfigFileName);
            if (FileUtils.IsFileExists(filePath))
            {
                var json = FileUtils.ReadText(filePath);
                if (json.Contains(@"""SecurityKey"": """","))
                {
                    var securityKey = StringUtils.GetShortGuid(false) + StringUtils.GetShortGuid(false) + StringUtils.GetShortGuid(false);
                    FileUtils.WriteText(filePath, json.Replace(@"""SecurityKey"": """",", $@"""SecurityKey"": ""{securityKey}"","));
                }
            }
            else
            {
                var securityKey = StringUtils.GetShortGuid(false) + StringUtils.GetShortGuid(false) + StringUtils.GetShortGuid(false);

                SaveSettings(contentRootPath, false, false, securityKey, DatabaseType.MySql.GetValue(),
                    string.Empty, string.Empty, string.Empty, null, null);
            }
        }

        public static string GetDatabaseConnectionString(DatabaseType databaseType, string server, bool isDefaultPort, int port, string userName, string password, string databaseName)
        {
            var connectionString = string.Empty;

            if (databaseType == DatabaseType.MySql)
            {
                connectionString = $"Server={server};";
                if (!isDefaultPort && port > 0)
                {
                    connectionString += $"Port={port};";
                }
                connectionString += $"Uid={userName};Pwd={password};";
                if (!string.IsNullOrEmpty(databaseName))
                {
                    connectionString += $"Database={databaseName};";
                }
                connectionString += "SslMode=Preferred;CharSet=utf8;";
            }
            else if (databaseType == DatabaseType.SqlServer)
            {
                connectionString = $"Server={server};";
                if (!isDefaultPort && port > 0)
                {
                    connectionString = $"Server={server},{port};";
                }
                connectionString += $"Uid={userName};Pwd={password};";
                if (!string.IsNullOrEmpty(databaseName))
                {
                    connectionString += $"Database={databaseName};";
                }
            }
            else if (databaseType == DatabaseType.PostgreSql)
            {
                connectionString = $"Host={server};";
                if (!isDefaultPort && port > 0)
                {
                    connectionString += $"Port={port};";
                }
                connectionString += $"Username={userName};Password={password};";
                if (!string.IsNullOrEmpty(databaseName))
                {
                    connectionString += $"Database={databaseName};";
                }
            }
            else if (databaseType == DatabaseType.SQLite)
            {
                connectionString = $"Data Source=~/{Constants.DefaultLocalDbFileName};Version=3;";
            }

            return connectionString;
        }

        public static string GetRedisConnectionString(string redisHost, bool isRedisDefaultPort, int redisPort, bool isSsl, string redisPassword)
        {
            var connectionString = string.Empty;
            if (!string.IsNullOrEmpty(redisHost))
            {
                var port = 6379;
                if (!isRedisDefaultPort && redisPort > 0)
                {
                    port = redisPort;
                }
                connectionString = $"{redisHost}:{port},allowAdmin=true";
                if (isSsl)
                {
                    connectionString += ",ssl=true";
                }
                if (!string.IsNullOrEmpty(redisPassword))
                {
                    connectionString += $",password={redisPassword}";
                }
            }

            return connectionString;
        }
    }
}
