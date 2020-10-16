﻿using System.Threading.Tasks;
using SSCMS.Models;

namespace SSCMS.Web.Controllers.Admin
{
    public partial class LoginController
    {
        public class CheckRequest
        {
            public string Token { get; set; }
            public string Value { get; set; }
        }

        public class GetResult
        {
            public bool Success { get; set; }
            public string RedirectUrl { get; set; }
            public string Version { get; set; }
            public string AdminTitle { get; set; }
        }

        public class LoginRequest
        {
            public string Account { get; set; }
            public string Password { get; set; }
            public bool IsPersistent { get; set; }
        }

        public class LoginResult
        {
            public Administrator Administrator { get; set; }
            public string SessionId { get; set; }
            public bool IsEnforcePasswordChange { get; set; }
            public string Token { get; set; }
        }

        private async Task<string> AdminRedirectCheckAsync()
        {
            var redirect = false;
            var redirectUrl = string.Empty;

            var config = await _configRepository.GetAsync();

            if (string.IsNullOrEmpty(_settingsManager.Database.ConnectionString))
            {
                redirect = true;
                redirectUrl = _pathManager.GetAdminUrl(InstallController.Route);
            }
            else if (config.Initialized &&
                     config.DatabaseVersion != _settingsManager.Version)
            {
                redirect = true;
                redirectUrl = _pathManager.GetAdminUrl(SyncDatabaseController.Route);
            }

            return redirect ? redirectUrl : null;
        }
    }
}
