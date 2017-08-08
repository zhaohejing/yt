using System.Linq;
using Abp.Configuration;
using Abp.Localization;
using Abp.Net.Mail;
using YT.Configuration;
using YT.EntityFramework;

namespace YT.Migrations.Seed.Host
{
    public class DefaultSettingsCreator
    {
        private readonly YtDbContext _context;

        public DefaultSettingsCreator(YtDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            AddSettingIfNotExists(YtSettings.General.MenuDefaultActive, "true");
            AddSettingIfNotExists(YtSettings.General.PermissionDefaultActive, "true");
        }

        private void AddSettingIfNotExists(string name, string value, int? tenantId = null)
        {
            if (_context.Settings.Any(s => s.Name == name && s.TenantId == tenantId && s.UserId == null))
            {
                return;
            }

            _context.Settings.Add(new Setting(tenantId, null, name, value));
            _context.SaveChanges();
        }
    }
}