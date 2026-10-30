using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using HRIS.Helper;
using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace HRIS.Areas.Master.Controllers
{
    [NoCache]
    [Authorize]
    public class  CalendarController : Controller
    {
        private static string optionName = "";
        #region internal methods
        public ActionResult DataForm(string screenName)
        {
            optionName = screenName;
            return View();
        }

        [HttpPost]
        public JsonResult Save(Calendar _mc)
        {
            var msg = new Object();
            try
            {
                if ((!_mc.PeriodId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_mc.PeriodId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region Validation
                    validate(_mc);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (Calendar.Save(_mc))
                        {
                            msg = new { success = true, message = AgilityEnums.Save_Success, formMode = AgilityEnums.formMode_Initialize };
                        }
                    }
                    else
                    {
                        msg = new
                        {
                            success = false,
                            message = ModelState.Keys.SelectMany(k => ModelState[k].Errors).Select(m => m.ErrorMessage).ToArray(),
                            formMode = AgilityEnums.formMode_Edit
                        };
                    }
                    #endregion
                }
                else
                {
                    msg = new { success = false, message = AgilityEnums.Save_Access_Denied, formMode = AgilityEnums.formMode_Edit };
                }
            }
            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Edit };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Get(short _keyId)
        {
            var msg = new Object(); 
            try
            {
                Calendar _mc =  Calendar.Get(_keyId);
                if (_mc == null)
                {
                    throw new Exception("The Calendar doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true,  calendar = _mc, formMode = AgilityEnums.formMode_ReadOnly };
            }

            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Recall };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
               
        #endregion

        #region Json Method
        [HttpGet]
        public JsonResult GetLookups()
        {
            try
            {
                return Json(new
                { success = true, lstPeriod = Period.Get() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private methods
        private void validate(Calendar _cal)
        {
            if (_cal.Details.Where(x => x.Holiday && string.IsNullOrEmpty(x.Reason)).Any())
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Reason for Holiday"));
        }
        #endregion
    }
}

