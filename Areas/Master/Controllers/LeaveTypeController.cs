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
    public class LeaveTypeController : Controller
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
                return Json(new { success = true, leaveType = new LeaveType(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(LeaveType _lt)
        {
            var msg = new Object();
            try
            {
                if ((!_lt.LeaveTypeId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_lt.LeaveTypeId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region Validation
                    validate(_lt);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (LeaveType.Save(_lt))
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
                    msg = new { success = false, message = "Sorry, you don't have required access to Save this Entry", formMode = AgilityEnums.formMode_Edit };
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
                LeaveType _lt = LeaveType.Get(_keyId);
                if (_lt == null)
                {
                    throw new Exception("The Leave Type Id doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, leaveType = _lt, formMode = AgilityEnums.formMode_ReadOnly };
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
                    columnDefs = ColumnDefinition.getColDef(new LeaveTypeList()),
                    rowData = LeaveTypeList.Get(false)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private methods
        private void validate(LeaveType _lt)
        {
            if (string.IsNullOrEmpty(_lt.LeaveTypeName))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Leave Type Desc"));

            if (_lt.DefaultDays < 0)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixgtZero("Default Days"));
        }
        #endregion
    }
}
