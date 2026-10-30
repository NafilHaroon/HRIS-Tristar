using HRIS.Areas.Download.Models;
using HRIS.Helper;
using System;
using System.Web.Mvc;

namespace HRIS.Areas.Download.Controllers
{
    [NoCache]
    [Authorize]
    public class EmployeePersonalController : Controller
    {
        private static string optionName = "";
        #region internal methods
        public ActionResult DataForm(string screenName)
        {
            optionName = screenName;
            return View();
        }

        [HttpGet]
        public JsonResult Get()
        {
            var msg = new Object(); 
            try
            {
                EmployeePersonal _ep = EmployeePersonal.Get();
                if (_ep == null)
                {
                    throw new Exception("No data exist on selected parameters or you donot have permission to access it");
                }
                msg = new { success = true, empPersonal = _ep, formMode = AgilityEnums.formMode_ReadOnly };
            }

            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Recall };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}

