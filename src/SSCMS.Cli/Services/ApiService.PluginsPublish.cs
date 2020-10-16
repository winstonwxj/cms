﻿using RestSharp;
using SSCMS.Core.Plugins;
using SSCMS.Utils;

namespace SSCMS.Cli.Services
{
    public partial class ApiService
    {
        public (bool success, string failureMessage) PluginsPublish(string publisher, string zipPath)
        {
            var status = _configService.Status;
            if (status == null || string.IsNullOrEmpty(status.UserName) || string.IsNullOrEmpty(status.AccessToken))
            {
                return (false, "you have not logged in");
            }

            if (status.UserName != publisher)
            {
                return (false, $"the publisher in package.json should be '{status.UserName}'");
            }

            var client = new RestClient(CloudUtils.Api.GetCliUrl(RestUrlPluginPublish)) { Timeout = -1 };
            var request = new RestRequest(Method.POST);
            //request.AddHeader("Content-Type", "multipart/form-data");
            request.AddHeader("Authorization", $"Bearer {status.AccessToken}");
            request.AddFile("file", zipPath);
            var response = client.Execute(request);

            return response.IsSuccessful ? (true, null) : (false, StringUtils.Trim(response.Content, '"'));
        }
    }
}
