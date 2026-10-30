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
    public class AttendanceController : Controller
    {
        private static string optionName = "";
        #region internal methods
        public ActionResult DataForm(string screenName)
        {
            optionName = screenName;
            return View();
        }

        [HttpPost]
        public JsonResult Save(Attendance _att)
        {
            var msg = new Object();
            try
            {
                if ((!_att.PeriodId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_att.PeriodId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region validation
                    validate(_att);
                    #endregion

                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (Attendance.Save(_att))
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

        public JsonResult Get(short _employeeId)
        {
            var msg = new Object(); 
            try
            {
                Attendance _att = Attendance.Get(_employeeId);
                if (_att == null)
                {
                    throw new Exception("Entries for attendance doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, attendance = _att, formMode = AgilityEnums.formMode_ReadOnly };
            }

            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Recall };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
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
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private methods
        private void validate(Attendance _att)
        {
            if (!_att.EmployeeId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Employee"));

            if (_att.Details.Count == 0)
                ModelState.AddModelError("Validation", "=> Attendance Grid cannot be empty");

            if (_att.Details.Count > 0)
            {
                if (_att.Details.Where(x => !x.AttendanceDate.HasValue).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Attendance Date"));

               else if (_att.Details.Where(x => (x.TimeIn.HasValue && !x.TimeOut.HasValue) || (!x.TimeIn.HasValue && x.TimeOut.HasValue)).Any())
                    ModelState.AddModelError("Validation", "Both Time In & Time Out must be mentioned or left blank!");

               else if (_att.Details.Where(x => x.AttendanceDate.HasValue).Any())
                {
                    if (_att.Details.GroupBy(x => x.AttendanceDate).Any(grp => grp.Count() > 1))
                        ModelState.AddModelError("Validation", "=> Attendance Date must be unique");
                }
                else if (_att.Details.Where(x => x.TimeIn.HasValue && x.TimeOut.HasValue).Any())                    
                {
                    if (_att.Details.Where(x => x.TimeIn.Value > x.TimeOut.Value).Any())
                        ModelState.AddModelError("Validation", "=> Time In must always be older than Time out");
                }     
            }
        }
        #endregion
    }
}