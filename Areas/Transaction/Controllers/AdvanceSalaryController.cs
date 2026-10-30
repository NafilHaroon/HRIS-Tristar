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
    public class AdvanceSalaryController : Controller
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
                return Json( new { success = true, advanceSalary = new Advance(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(Advance _adv)
        {
            var msg = new Object();
            try
            {
                if ((!_adv.AdvanceId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_adv.AdvanceId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region validation
                    validate(_adv);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (Advance.Save(_adv))
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
                            session = "Edit"
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
                msg = new { success = false, message = ex.Message, session = "Edit" };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Get(short _keyId)
        {
            var msg = new Object();
            try
            {
                Advance _as = Advance.Get(_keyId);
                if (_as == null)
                {
                    throw new Exception("The Advance Salary Ref # doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, advanceSalary = _as, formMode = AgilityEnums.formMode_ReadOnly};
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
                    columnDefs = ColumnDefinition.getColDef(new AdvanceList()),
                    rowData = AdvanceList.Get()
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
            return Json(EmployeeShortList.Get(), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region private methods
        private void validate(Advance _as)
        {
            if (_as.StatusName != "Open")
                ModelState.AddModelError("Validation", "=> No further changes can be made to this Advance Salary");

            if (!_as.Employee.EmployeeId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Employee"));

            if (!_as.AdvanceDate.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Advance Date"));

            if (string.IsNullOrEmpty(_as.Description))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Description"));

            if (_as.Amount == 0)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixNZ("Amount"));
        }
        #endregion
    }
}