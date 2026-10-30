using HRIS.Areas.Common.Models;
using HRIS.Areas.Process.Models;
using HRIS.Helper;
using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace HRIS.Areas.Process.Controllers
{
    [NoCache]
    [Authorize]
    public class CloseWIPSalaryController : Controller
    {
        private static string optionName = "";
        #region internal methods
        public ActionResult DataForm(string screenName)
        {
            optionName = screenName;
            return View();
        }

        [HttpPost]
        public JsonResult Save(CloseWIPSalary _cws)
        {
            var msg = new Object();
            try
            {
                if (ServerSession.EditAllowed(optionName))
                {
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (CloseWIPSalary.Close(_cws))
                        {
                            msg = new { success = true, message = "Selected Salary entries are marked as Verified & Closed", formMode = AgilityEnums.formMode_Initialize };
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
        [HttpGet]
        public JsonResult Get()
        {
            var msg = new Object(); 
            try
            {
                CloseWIPSalary _cms = CloseWIPSalary.Get();
                if (_cms == null)
                {
                    throw new Exception("No entry exist in WIP Salary section or you donot have permission to access it");
                }
                msg = new { success = true, wipSalary = _cms, formMode = AgilityEnums.formMode_ReadOnly };
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

