using System.Web.Mvc;

namespace HRIS.Areas.PF
{
    public class PFAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "PF";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "PF_default",
                "PF/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
