using System.Web.Mvc;

namespace HRIS.Areas.Common
{
    public class CommonAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Common";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
               "Common_default",
               "Common/{controller}/{action}/{id}",
               new { action = "Index", id = UrlParameter.Optional },
               namespaces: new[] { "HRIS.Areas.Common.Controllers" }
           );//.DataTokens.Add("area", "Common");
        }
    }
}
