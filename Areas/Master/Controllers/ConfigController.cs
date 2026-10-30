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
    public class ConfigController : Controller
    {
        private static string optionName = "";
        #region public methods
        public ActionResult DataForm(string screenName)
        {
            optionName = screenName;
            return View();
        }

        [HttpPost]
        public JsonResult Save(Config _cfg)
        {
            var msg = new Object();
            try
            {
                if (ServerSession.EditAllowed(optionName))
                {
                    #region Validation
                    validate(_cfg);
                    #endregion

                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (Config.Save(_cfg))
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

        [HttpGet]
        public JsonResult Get()
        {
            var msg = new Object(); 
            try
            {
                Config _cn = Config.Get();
                if (_cn == null)
                {
                    throw new Exception("The Config  doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, config = _cn, formMode = AgilityEnums.formMode_ReadOnly };
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
                {
                    success = true,
                    lstLoan = SalaryHeads.Get(true).Where(x => x.HeadTypeId == 4).ToList(),
                    lstAbsent = SalaryHeads.Get(true).Where(x => x.HeadTypeId == 3).ToList()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private methods
        private void validate(Config _c)
        {
            if (!_c.AbsentHeadId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Absent Deduction"));

            if (!_c.AdvanceHeadId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Advance Deduction"));

            if (!_c.LoanDeductionId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Loan Deduction"));
        }
        #endregion
    }
}