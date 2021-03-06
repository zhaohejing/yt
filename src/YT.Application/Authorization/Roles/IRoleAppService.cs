using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using YT.Authorization.Roles.Dto;

namespace YT.Authorization.Roles
{
    /// <summary>
    /// Application service that is used by 'role management' page.
    /// </summary>
    public interface IRoleAppService : IApplicationService
    {/// <summary>
    /// 分页获取角色信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
        Task<ListResultDto<RoleListDto>> GetRoles(GetRolesInput input);
        /// <summary>
        /// 获取角色+权限信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<GetRoleForEditOutput> GetRoleForEdit(NullableIdDto input);
        /// <summary>
        /// 创建或编辑角色
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task CreateOrUpdateRole(CreateOrUpdateRoleInput input);
        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task DeleteRole(EntityDto input);
    }
}