﻿using Newtonsoft.Json;
using SSCMS.Services;

namespace SSCMS.Cli.Updater.Tables
{
    public partial class TableChannelGroup
    {
        private readonly IDatabaseManager _databaseManager;

        public TableChannelGroup(IDatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("nodeGroupName")]
        public string NodeGroupName { get; set; }

        [JsonProperty("publishmentSystemID")]
        public long PublishmentSystemId { get; set; }

        [JsonProperty("taxis")]
        public long Taxis { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
