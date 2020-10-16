﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SSCMS.Configuration;
using SSCMS.Dto;
using SSCMS.Repositories;
using SSCMS.Services;

namespace SSCMS.Web.Controllers.Admin.Settings.Administrators
{
    [OpenApiIgnore]
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class AdministratorsRoleController : ControllerBase
    {
        private const string Route = "settings/administratorsRole";

        private readonly IAuthManager _authManager;
        private readonly IRoleRepository _roleRepository;
        private readonly ISitePermissionsRepository _sitePermissionsRepository;
        private readonly IPermissionsInRolesRepository _permissionsInRolesRepository;

        public AdministratorsRoleController(IAuthManager authManager, IRoleRepository roleRepository, ISitePermissionsRepository sitePermissionsRepository, IPermissionsInRolesRepository permissionsInRolesRepository)
        {
            _authManager = authManager;
            _roleRepository = roleRepository;
            _sitePermissionsRepository = sitePermissionsRepository;
            _permissionsInRolesRepository = permissionsInRolesRepository;
        }

        [HttpGet, Route(Route)]
        public async Task<ActionResult<ListRequest>> GetList()
        {
            if (!await _authManager.HasAppPermissionsAsync(Types.AppPermissions.SettingsAdministratorsRole))
            {
                return Unauthorized();
            }

            var roleInfoList = await _authManager.IsSuperAdminAsync()
                ? await _roleRepository.GetRolesAsync()
                : await _roleRepository.GetRolesByCreatorUserNameAsync(_authManager.AdminName);

            var roles = roleInfoList.Where(x => !_roleRepository.IsPredefinedRole(x.RoleName)).ToList();

            return new ListRequest
            {
                Roles = roles
            };
        }

        [HttpDelete, Route(Route)]
        public async Task<ActionResult<ListRequest>> Delete([FromBody] IdRequest request)
        {
            if (!await _authManager.HasAppPermissionsAsync(Types.AppPermissions.SettingsAdministratorsRole))
            {
                return Unauthorized();
            }

            var roleInfo = await _roleRepository.GetRoleAsync(request.Id);

            await _permissionsInRolesRepository.DeleteAsync(roleInfo.RoleName);
            await _sitePermissionsRepository.DeleteAsync(roleInfo.RoleName);
            await _roleRepository.DeleteRoleAsync(roleInfo.Id);

            await _authManager.AddAdminLogAsync("删除管理员角色", $"角色名称:{roleInfo.RoleName}");

            var roles = await _authManager.IsSuperAdminAsync()
                ? await _roleRepository.GetRolesAsync()
                : await _roleRepository.GetRolesByCreatorUserNameAsync(_authManager.AdminName);

            return new ListRequest
            {
                Roles = roles
            };
        }
    }
}
