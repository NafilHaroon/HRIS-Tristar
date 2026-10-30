using HRIS.Areas.Download.Models;
using HRIS.Areas.Master.Models;
using HRIS.Helper;
using System;
using System.Web.Mvc;

namespace HRIS.Areas.Download.Controllers
{
    [NoCache]
    [Authorize]
    public class S21JVController : Controller
    {
        //private static string optionName = "";
        #region internal methods
        public ActionResult DataForm(string screenName)
        {
          //  optionName = screenName;
            return View();
        }

        [HttpGet]
        public JsonResult GetLookups()
        {
            try
            {
                return Json(new
                {
                    lstPeriod = Period.GetClosedPeriods()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult Get(short _periodId)
        {
            var msg = new Object(); 
            try
            {
                S21JV _s21 = S21JV.Get(_periodId);
                if (_s21 == null)
                {
                    throw new Exception("No data exist on selected parameters or you donot have permission to access it");
                }
                msg = new { success = true, s21JV = _s21, formMode = AgilityEnums.formMode_ReadOnly};
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

