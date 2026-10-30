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
    public class LeaveController : Controller
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
                return Json(new { success = true, leave = new Leave(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(Leave _lv)
        {
            var msg = new Object();
            try
            {
                if ((string.IsNullOrWhiteSpace(_lv.AuditTrail.CreatedBy) && ServerSession.AddAllowed(optionName)) ||
                      (!string.IsNullOrWhiteSpace(_lv.AuditTrail.CreatedBy) && ServerSession.EditAllowed(optionName)))
                {
                    #region Validation
                    validate(_lv);
                    #endregion

                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (Leave.Save(_lv))
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
                Leave _lv = Leave.Get(_keyId);
                if (_lv == null)
                {
                    throw new Exception("The Leave Ref # entered doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, leave = _lv, formMode = AgilityEnums.formMode_ReadOnly };
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
                    columnDefs = ColumnDefinition.getColDef(new LeaveList()),
                    rowData = LeaveList.Get()
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
                    lstEmployee = EmployeeShortList.Get(),
                    lstLeaveType = LeaveTypeList.Get(true),
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private methods
        private void validate(Leave _lv)
        {
            if (!_lv.EmployeeId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Employee"));

            if (!_lv.LeaveTypeId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Leave Type"));

            if (!_lv.DateFrom.HasValue & !_lv.DateTo.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Leave Date From & To"));
            else
            {
                if (_lv.DateFrom.Value.Date > _lv.DateTo.Value.Date)
                    ModelState.AddModelError("Validation", AgilityHelper.Suffixgt("Date To", "Date From"));

                if (_lv.HalfDay && _lv.DateFrom.Value.Date != _lv.DateTo.Value.Date)
                    ModelState.AddModelError("Validation", "Date From & To must be same if leave is marked as Half Day");
            }

            if (string.IsNullOrWhiteSpace(_lv.Reason))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Leave Reason"));
        }
        #endregion
    }
}

