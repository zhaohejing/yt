using Abp.Zero.Ldap.Authentication;
using Abp.Zero.Ldap.Configuration;
using YT.Authorization.Users;
using YT.MultiTenancy;

namespace YT.Authorization.Ldap
{
    public class AppLdapAuthenticationSource : LdapAuthenticationSource<Tenant, User>
    {
        public AppLdapAuthenticationSource(ILdapSettings settings, IAbpZeroLdapModuleConfig ldapModuleConfig)
            : base(settings, ldapModuleConfig)
        {
        }
    }
}
