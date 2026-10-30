using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
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
    public class CoAController : Controller
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
                return Json(new { success = true, CoA = new CoA(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(CoA _coa)
        {
            var msg = new Object();
            try
            {
                if ((!_coa.AccountId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_coa.AccountId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region Validation
                    validate(_coa);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (CoA.Save(_coa))
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
        public JsonResult Get(string _keyId)
        {
            var msg = new Object();
            try
            {
                CoA _coa = CoA.Get(_keyId);
                if (_coa == null)
                {
                    throw new Exception("The Account Code doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, CoA = _coa, formMode = AgilityEnums.formMode_ReadOnly };
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
                    columnDefs = ColumnDefinition.getColDef(new CoAList()),
                    rowData = CoAList.Get()
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
                    lstAccountType = AccountTypes.Get()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region private methods
        private void validate(CoA _coa)
        {
            if (string.IsNullOrEmpty(_coa.AccountName))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Account Name"));

            if (!_coa.AccountTypeId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Account Type"));

            if (!_coa.OpeningBalance.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixZero("Opening Balance"));
        }
        #endregion
    }
}


