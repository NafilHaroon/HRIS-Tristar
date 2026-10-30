using System.Web.Mvc;

namespace HRIS.Areas.Master
{
    public class MasterAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Master";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
             context.MapRoute(
               "Master_default",
               "Master/{controller}/{action}/{id}",
               new { action = "DataForm", id = UrlParameter.Optional },
               namespaces: new[] { "HRIS.Areas.Master.Controllers" }
           );
        }
    }
}
