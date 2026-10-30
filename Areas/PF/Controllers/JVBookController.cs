using HRIS.Areas.Common.Models;
using HRIS.Areas.PF.Models;
using HRIS.Helper;
using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace HRIS.Areas.PF.Controllers
{
    [NoCache]
    [Authorize]
    public class JVBookController : Controller
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
                return Json(new { success = true, jv= new JVBook(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(JVBook _jv)
        {
            var msg = new Object();
            try
            {
                if (ServerSession.AddAllowed(optionName))
                {
                    #region Validation
                    validate(_jv);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (JVBook.Save(_jv))
                        {
                            msg = new { success = true, message = "Journal Voucher # " + _jv.VoucherNo + " saved successfully", formMode = AgilityEnums.formMode_Initialize };
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
        public JsonResult Get(string _keyId)
        {
            var msg = new Object(); 
            try
            {
                JVBook _jv = JVBook.Get(_keyId);
                if (_jv == null)
                {
                    throw new Exception("The Voucher # doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, jv = _jv, formMode = AgilityEnums.formMode_ReadOnly };
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
                    columnDefs = ColumnDefinition.getColDef(new JVBookList()),
                    rowData = JVBookList.Get()
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
                    success = true,
                    lstAccount = CoAList.GetCoAsForDropdown()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private methods
        private void validate(JVBook _jv)
        {
            if (!string.IsNullOrEmpty(_jv.VoucherNo))
                ModelState.AddModelError("Validation", "=> No further changes can be made to this Journal Voucher at this stage");

            if (!_jv.VoucherDate.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Voucher Date"));

            if (_jv.Details.Count < 2)
            {
                ModelState.AddModelError("Validation", "Atleast couple of Line Items must exist before calling save operation");
            }
            else
            {
                if (_jv.Details.Where(x => !x.AccountId.HasValue).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Account"));

                if (_jv.Details.GroupBy(x => x.AccountId).Any(grp => grp.Count() > 1))
                    ModelState.AddModelError("Validation", "Accounts used in specific JV Entry must be unique");

                if (_jv.Details.Where(x => x.Debit <0 || x.Credit < 0).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixZero("Debit or Credit"));

                if (_jv.Details.Where(x => x.Debit == 0 && x.Credit == 0).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixgtZero("Debit or Credit"));

                if (_jv.Details.Where(x => x.Debit != 0 && x.Credit != 0).Any())
                    ModelState.AddModelError("Validation", "One line item can only have either Debit or Credit as non-Zero");

                if (_jv.Details.Sum(x=>x.Debit) != _jv.Details.Sum(y=>y.Credit))
                    ModelState.AddModelError("Validation", "The sum of Debit and Credit entries must be equal. Please make missing entries to adjust both sides before saving the entry!");
            }
        }
        #endregion
    }
}


