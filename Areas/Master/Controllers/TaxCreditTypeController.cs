using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using HRIS.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace HRIS.Areas.Master.Controllers
{
    [NoCache]
    [Authorize]
    public class TaxCreditTypeController : Controller
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
                return Json(new { success = true, creditType = new TaxCreditType(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    errorMsg = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(TaxCreditType _ct)
        {
            var msg = new Object();
            try
            {
                if ((!_ct.TaxCreditTypeId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_ct.TaxCreditTypeId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region Validation
                    validate(_ct);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (TaxCreditType.Save(_ct))
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
                TaxCreditType _ct = TaxCreditType.Get(_keyId);
                if (_ct == null)
                {
                    throw new Exception("The Tax Credit type doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, creditType = _ct, formMode = AgilityEnums.formMode_ReadOnly};
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
                _data = new { success = true, columnDefs = ColumnDefinition.getColDef(new TaxCreditTypeList()), rowData = TaxCreditTypeList.Get(false) };
                return Json(_data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _data = new { success = false, message = ex.Message };
                return Json(_data, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetLookups()
        {
            return Json(new { lstCreditRule = TaxCreditRule.Get() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region private methods           
        private void validate(TaxCreditType dat)
        {
            if (string.IsNullOrEmpty(dat.TaxCreditTypeName))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Tax Credit Type Name"));
        }
        #endregion
    }
}
