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
    public class WIPSalaryController : Controller
    {
        private static string optionName = "";
        #region internal methods
        public ActionResult DataForm(string screenName)
        {
            optionName = screenName;
            Session.Remove("lstSalaryHead");
            return View();
        }

        [HttpGet]
        public JsonResult Add()
        {
            try
            {
                return Json(new { success = true, wipSalary = new WIPSalary(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(WIPSalary _ws)
        {
            var msg = new Object();
            try
            {
                if ((!_ws.SalaryId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_ws.SalaryId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region Validation
                    validate(_ws);
                #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (WIPSalary.Save(_ws))
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
                WIPSalary _ws = WIPSalary.Get(_keyId);
                if (_ws == null)
                {
                    throw new Exception("The Monthly Salary Id doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, wipSalary = _ws, formMode = AgilityEnums.formMode_ReadOnly };
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
            try
            {
                return Json(new
                {
                    success = true,
                    columnDefs = ColumnDefinition.getColDef(new WIPSalaryList()),
                    rowData = WIPSalaryList.Get()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult Delete(short _keyId)
        {
            try
            {
                if (ServerSession.DeleteAllowed(optionName))
                {
                    if (WIPSalary.Delete(_keyId))
                    {
                        return Json(new { success = true, message = "Salary detail for Id " + _keyId + " deleted successfully", formMode = AgilityEnums.formMode_Initialize }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { success = false, message = "You don't have access to delete function on WIP Salary", formMode = AgilityEnums.formMode_Edit }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Edit }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetLookups()
        {
            try
            {
                //this is done as this session is referred in validation method
                if (Session["lstSalaryHead"] == null)
                    Session["lstSalaryHead"] = SalaryHeads.Get(true);
                return Json(new
                {
                    success = true,
                    lstEmployee = EmployeeShortList.GetForWIPSalary(),
                    lstConsumable = EntitlementTypes.GetConsumables(),
                    lstSalaryHead = (List<SalaryHeads>) Session["lstSalaryHead"]
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private methods
        private void validate(WIPSalary _ws)
        {
            if (!_ws.EmployeeId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Employee"));

            if (_ws.LeaveDays > _ws.SalaryDays)
                ModelState.AddModelError("Validation", "Leave days can't exceed Total Salary days in the month");

            if (_ws.UnpaidDays > _ws.SalaryDays)
                ModelState.AddModelError("Validation", "Absent days can't exceed Total Salary days in the month");

            if (_ws.LeaveDays + _ws.UnpaidDays > _ws.SalaryDays)
                ModelState.AddModelError("Validation", "Absent & Leave days can't exceed Total Salary days in the month");

            if (_ws.Salary.Where(x => !x.Delete).Count() == 0)
            {
                ModelState.AddModelError("Validation", "There must be atleast one row exist in Salary Grid!");
            }
            else
            {
                if (_ws.Salary.Where(x => !x.Delete).Where(x => !x.HeadId.HasValue).Any())
                    ModelState.AddModelError("Validation", "Each row in Salary Setup must have valid Salary Head selected!");

                if (_ws.Salary.Where(x => !x.Delete && x.Amount <= 0).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixgtZero("Amount"));

                if (_ws.Salary.Where(x => !x.Delete && !x.SystemCalculated).GroupBy(x => new { x.HeadId }).Any(grp => grp.Count() > 1))
                    ModelState.AddModelError("Validation", "Salary Head must be unique");

                if ((from _s in _ws.Salary.Where(x=>!x.Delete)
                     join _sh in ((List<SalaryHeads>)Session["lstSalaryHead"]) on _s.HeadId equals _sh.HeadId
                     select new { Amt = _sh.Symbol * _s.Amount }).Sum(x => x.Amt) < 0)
                    ModelState.AddModelError("Validation", "The total of Salary heads must be greater than zero!");
            }

            if (_ws.Consumable.Where(x => !x.Delete).Count() > 0)
            {
                if (_ws.Consumable.Where(x => !x.Delete).Where(x => !x.EntitlementTypeId.HasValue).Any())
                    ModelState.AddModelError("Validation", "Employee Entitlements entries must have valid Entitlement Type selected!");

                if (_ws.Consumable.Where(x => x.Limit <= 0).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixgtZero("Limit"));

                if (_ws.Consumable.Where(x => !x.Delete).GroupBy(x => x.EntitlementTypeId).Any(grp => grp.Count() > 1))
                    ModelState.AddModelError("Validation", "=> Entitlement Type used in Entitlement List must be unique");
            }
        }
        #endregion
    }
}

