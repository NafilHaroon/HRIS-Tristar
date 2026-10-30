using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using HRIS.Helper;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HRIS.Areas.Common.Controllers
{
    [NoCache]
    [Authorize]
    public class CommonController : Controller
    {
        public CommonController()
        {
            
        }

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToActionPermanent("Login", "User");
            }
            else if (Session["MainMenu"] == null)
            {
                Session["MainMenu"] = Menu.Get();
            }
            ViewBag.Menu = (List<Menu>)Session["MainMenu"];
            return View("MainLayout");
        }

        public ActionResult Close()
        {
            return View();
        }
    }
}
