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
    public class BankController : Controller
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
                return Json(new { success = true, bank = new Bank(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(Bank _b)
        {
            var msg = new Object();
            try
            {
                if ((!_b.BankId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_b.BankId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region Validation
                    validate(_b);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (Bank.Save(_b))
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
                Bank _b = Bank.Get(_keyId);
                if (_b == null)
                {
                    throw new Exception("The Bank Id doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, bank = _b, formMode = AgilityEnums.formMode_ReadOnly };
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
                return Json(new { success = true, columnDefs = ColumnDefinition.getColDef(new Banks()),
                    rowData = Banks.Get(false) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetLookups()
        {
            return Json(CompanyList.Get(true), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region private methods
        private void validate(Bank _b)
        {
            if (string.IsNullOrEmpty(_b.BankName))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Bank Name"));

            if (!string.IsNullOrEmpty(_b.BankName))
            {
                if (!Regex.IsMatch(_b.ContactPerson, "^[a-zA-Z -/]+$"))
                    ModelState.AddModelError("Validation", "=> Bank Name must carry only alphabets");
            }

            if (string.IsNullOrEmpty(_b.AccountNo))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Account #"));

            if (string.IsNullOrEmpty(_b.Address))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Address"));

            if (string.IsNullOrEmpty(_b.ContactPerson))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("ContactPerson"));

            if (!string.IsNullOrEmpty(_b.ContactPerson))
            {
              if (!Regex.IsMatch(_b.ContactPerson, "^[a-zA-Z -/]+$"))
                    ModelState.AddModelError("Validation", "=> Contact Person must carry only alphabets");
            }
        }
        #endregion
    }
}
