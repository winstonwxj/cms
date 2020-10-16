﻿namespace SSCMS.Cli.Services
{
    public partial class ApiService
    {
        public (bool success, PluginAndUser result, string failureMessage) PluginsShow(string pluginId)
        {
            //var client = new RestClient(CloudUtils.Api.GetCliUrl(RestUrlPluginShow)) { Timeout = -1 };
            //var request = new RestRequest(Method.POST);
            //request.AddHeader("Content-Type", "application/json");
            //request.AddParameter("application/json", TranslateUtils.JsonSerialize(new ShowRequest
            //{
            //    PluginId = pluginId
            //}), ParameterType.RequestBody);
            //var response = client.Execute<PluginAndUser>(request);
            //if (!response.IsSuccessful)
            //{
            //    return (false, null, response.ErrorMessage);
            //}

            //return (true, response.Data, null);

            return ExecutePost<ShowRequest, PluginAndUser>(RestUrlPluginShow, new ShowRequest
            {
                PluginId = pluginId
            });
        }
    }
}
