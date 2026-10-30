using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using HRIS.Helper;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace HRIS.Areas.Master.Controllers
{
    [NoCache]
    [Authorize]
    public class SalaryHeadController : Controller
    {
        private static string optionName = "";
        #region internal methods
        public ActionResult DataForm(string screenName)
        {
            optionName = screenName;
            return View();
        }

        [HttpGet]
        public JsonResult Add()
        {
            try
            {
                ModelState.Clear();
                return Json(new { success = true, salaryHead = new SalaryHead(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(SalaryHead _sh)
        {
            var msg = new Object();
            try
            {
                if ((!_sh.HeadId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_sh.HeadId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region Validation
                    validate(_sh);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (SalaryHead.Save(_sh))
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
        public JsonResult Get(short _keyId)
        {
            var msg = new Object();
            try
            {
                SalaryHead _sh = SalaryHead.Get(_keyId);
                if (_sh == null)
                {
                    throw new Exception("The Salary Head Id doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, salaryHead = _sh, formMode = AgilityEnums.formMode_ReadOnly };
            }

            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Recall };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Search()
        {
            var _data = new object();
            try
            {
                return Json(new
                {
                    success = true,
                    columnDefs = ColumnDefinition.getColDef(new SalaryHeads()),
                    rowData = SalaryHeads.Get(false)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
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
                    lstHeadType = SalaryHeadTypes.Get(),
                    lstWorkerType = WorkerTypes.Get()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private methods
        private void validate(SalaryHead _sh)
        {
            if (string.IsNullOrEmpty(_sh.HeadName))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Head Name"));

            if (!string.IsNullOrEmpty(_sh.HeadName))
            {
                if (!Regex.IsMatch(_sh.HeadName, "^[a-zA-Z -/]+$"))
                    ModelState.AddModelError("Validation", "=> Head Name can only carry alphabets");
            }

            if (!_sh.HeadTypeId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Head Type"));
            
            if (_sh.DefaultAmount < 0)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixZero("Default Amount"));

            if (_sh.Details.Count == 0)
            {
                ModelState.AddModelError("Validation", "=> Atleast one Worker Type must exist in Salary Head to perform save operation");
            }

            if (_sh.Details.Where(x => !x.WorkerTypeId.HasValue).Any())
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Worker Type"));

            if (_sh.Details.GroupBy(x => x.WorkerTypeId).Any(grp => grp.Count() > 1))
                ModelState.AddModelError("Validation", "=> Worker Type must be unique in the list");

            if (_sh.Details.Where(x => string.IsNullOrWhiteSpace(x.DebitAccount)).Any())
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Debit A/c"));

            if (_sh.Details.Where(x => !string.IsNullOrWhiteSpace(x.DebitAccount) && x.DebitAccount.Trim().Length != 6).Any())
                ModelState.AddModelError("Validation", "=> Lenght of Debit A/c must be exactly 6 digits");

            if (_sh.Details.Where(x => string.IsNullOrWhiteSpace(x.CreditAccount)).Any())
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Credit A/c"));

            if (_sh.Details.Where(x => !string.IsNullOrWhiteSpace(x.CreditAccount) && x.CreditAccount.Trim().Length != 6).Any())
                ModelState.AddModelError("Validation", "=> Lenght of Credit A/c must be exactly 6 digits");
        }
        #endregion
    }
}
