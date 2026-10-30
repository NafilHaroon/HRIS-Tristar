using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using HRIS.Areas.Transaction.Models;
using HRIS.Helper;
using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace HRIS.Areas.Transaction.Controllers
{
    [NoCache]
    [Authorize]
    public class SalaryCreditController : Controller
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
                return Json(new { success = true, salaryLetter = new SalaryCreditLetter(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(SalaryCreditLetter _scl)
        {
            var msg = new Object();
            try
            {
                if ((!_scl.LetterId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_scl.LetterId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region Validation
                    validate(_scl);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (SalaryCreditLetter.Save(_scl))
                        {
                            msg = new { success = true, message = "Salary Credit Letter successfully saved under Reference # " + _scl.LetterId.Value.ToString(), formMode = AgilityEnums.formMode_Initialize };
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
                SalaryCreditLetter _scl = SalaryCreditLetter.Get(_keyId);
                if (_scl == null)
                {
                    throw new Exception("The Salary Credit Letter # doesn't exist or you do not have permission to access it");
                }
                msg = new { success = true, salaryLetter = _scl, formMode = AgilityEnums.formMode_ReadOnly };
            }

            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Recall };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetPendingSalaries(short _bankId, short _periodId)
        {
            var msg = new Object();
            try
            {
                SalaryCreditLetter _scl = new SalaryCreditLetter
                {
                    BankId = _bankId,
                    PeriodId = _periodId,
                    Details = SalaryCreditDetail.GetPendingSalaries(_bankId, _periodId)
                };

                if (_scl == null)
                {
                    throw new Exception("No pending Salary exist for the combinbation of Selected Bank & Period OR you donot have permission to access pending employees");
                }
                msg = new { success = true, salaryLetter = _scl, formMode = AgilityEnums.formMode_Add };
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
                    columnDefs = ColumnDefinition.getColDef(new SCLList()),
                    rowData = SCLList.Get()
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
            try
            {
                return Json(new
                {
                    success = true,
                    lstPeriod = Period.GetClosedPeriods(),
                    lstBank = Banks.Get(true),
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private methods
        private void validate(SalaryCreditLetter _scl)
        {
            if (!_scl.BankId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Bank"));

            if (!_scl.PeriodId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Period"));

            if (_scl.Details.Count == 0)
                ModelState.AddModelError("Validation", "Please Select at least one employee to create Salary Credit Letter!");

            //if (blist.Where(x => x.StopSalary).Any())
              //  ModelState.AddModelError("Validation", "This salary has been stopped by HR therefore you can't include it in any letter unless it is released by the Authorized User. Please contact your HR or System Admin for further details");
        }
        #endregion
    }
}

