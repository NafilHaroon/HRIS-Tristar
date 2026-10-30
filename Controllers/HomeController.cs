using HRIS.Areas.Common.Models;
using HRIS.Helper;
using System.Web.Mvc;

namespace HRIS.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetAccessRight(string _optionName, string _formMode = "Initialize")
        {
            using (AccessRights _ar = ServerSession.GetAccessRights(_optionName, false))
            {
                _ar.FormMode = _formMode;
                return Json(_ar, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
