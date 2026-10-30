using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using HRIS.Areas.Transaction.Models;
using HRIS.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace HRIS.Areas.Transaction.Controllers
{
    [NoCache]
    [Authorize]
    public class LoanController : Controller
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
                return Json(new { success = true, loan= new Loan(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(Loan _ls)
        {
            var msg = new Object();
            try
            {
                if ((!_ls.LoanId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_ls.LoanId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region Validation
                    validate(_ls);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (Loan.Save(_ls))
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
                Loan _ls = Loan.Get(_keyId);
                if (_ls == null)
                {
                    throw new Exception("The Loan Schedule Id doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, loan = _ls, formMode = AgilityEnums.formMode_ReadOnly };
            }

            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Recall };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Search()
        {
            var _data = new object();
            try
            {
                return Json(new
                {
                    success = true,
                    columnDefs = ColumnDefinition.getColDef(new LoanList()),
                    rowData = LoanList.Get()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetLookups()
        {
            return Json(new
            {
                lstEmployee = EmployeeShortList.Get(),
                lstLoanType = LoanType.Get(),
                lstPeriod = Period.Get()
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region private methods
        private void validate(Loan _ls)
        {
            if (!_ls.Employee.EmployeeId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Employee"));

            if (!_ls.LoanTypeId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Loan Type"));

            if (!_ls.PeriodId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Effective Period"));

            if (string.IsNullOrEmpty(_ls.Reason))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Reason"));

            if (_ls.LoanAmount <= 0)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixgtZero("Loan Amount"));

            if (_ls.Installments <= 0)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixgtZero("Loan Installments"));
        }
        #endregion
    }
}

