using System.Web.Mvc;
using Abp.Auditing;
using Abp.Web.Mvc.Authorization;
using YT.Authorization;
using YT.Web.Controllers;

namespace YT.Web.Areas.Mpa.Controllers
{
    [DisableAuditing]
    [AbpMvcAuthorize(AppPermissions.Pages_Administration_AuditLogs)]
    public class AuditLogsController : AbpZeroTemplateControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}