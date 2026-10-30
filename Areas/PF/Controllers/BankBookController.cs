using HRIS.Areas.Common.Models;
using HRIS.Areas.PF.Models;
using HRIS.Helper;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace HRIS.Areas.PF.Controllers
{
    [NoCache]
    [Authorize]
    public class BankBookController : Controller
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
                return Json(new { success = true, bb = new BankBook(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(BankBook _bb)
        {
            var msg = new Object();
            try
            {
                if ((!_bb.VoucherId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_bb.VoucherId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region Validation
                    validate(_bb);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (BankBook.Save(_bb))
                        {
                            msg = new { success = true, message = "Bank Voucher # " + _bb.VoucherNo + " saved successfully", formMode = AgilityEnums.formMode_Initialize };
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
        public JsonResult Get(string _keyId)
        {
            var msg = new Object(); 
            try
            {
                BankBook _bb = BankBook.Get(_keyId);
                if (_bb == null)
                {
                    throw new Exception("The Bank Voucher # doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, bb = _bb, formMode = AgilityEnums.formMode_ReadOnly };
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
                    columnDefs = ColumnDefinition.getColDef(new BankBookList()),
                    rowData = BankBookList.Get()
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
                    lstBankAccount = CoAList.GetBankAccounts(),
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
        private void validate(BankBook _bb)
        {
            if (!string.IsNullOrEmpty(_bb.VoucherNo))
                ModelState.AddModelError("Validation", "=> No further changes can be made to this Bank Book at this stage");

            if (!_bb.VoucherDate.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Voucher Date"));

            if (!_bb.BankAccountId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Bank A/c"));

            if (string.IsNullOrEmpty(_bb.ChequeNo))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Cheque Number"));


            if (!string.IsNullOrEmpty(_bb.PayeeName))
            {
                if (!Regex.IsMatch(_bb.PayeeName, "^[a-zA-Z -/]+$"))
                    ModelState.AddModelError("Validation", "=> Payee Name must contain Alphabets only!");
            }

            if (!_bb.ChequeDate.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Cheque Date"));

            if (_bb.Details.Count == 0)
            {
                ModelState.AddModelError("Validation", "Atleast one Debit or Credit entry must exist to perform save operation");
            }
            else
            {
                if (_bb.Details.Where(x => !x.AccountId.HasValue).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Account"));

                if (_bb.Details.GroupBy(x => x.AccountId).Any(grp => grp.Count() > 1))
                    ModelState.AddModelError("Validation", "Account used in specific Bank Voucher must be unique");

                if (_bb.Details.Where(x => x.Debit < 0 || x.Credit < 0).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixZero("Debit or Credit"));

                if (_bb.Details.Where(x => x.Debit == 0 && x.Credit == 0).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixgtZero("Debit or Credit"));

                if (_bb.Details.Where(x => x.Debit != 0 && x.Credit != 0).Any())
                    ModelState.AddModelError("Validation", "One line item can only have either Debit or Credit as non-Zero");
            }
        }
        #endregion
    }
}


