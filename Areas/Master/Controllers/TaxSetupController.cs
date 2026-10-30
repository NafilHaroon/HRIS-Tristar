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
    public class TaxSetupController : Controller
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
                return Json(new { success = true, TaxSetup = new TaxSetup(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(TaxSetup _ts)
        {
            var msg = new Object();
            try
            {
                if (ServerSession.EditAllowed(optionName))
                {
                    #region Validation
                    validate(_ts);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (TaxSetup.Save(_ts))
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
                TaxSetup _ts = TaxSetup.Get(_keyId);
                if (_ts == null)
                {
                    throw new Exception("The Tax Year doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, TaxSetup = _ts, formMode = AgilityEnums.formMode_ReadOnly };
            }

            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Recall };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
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
                    lstTaxYear = TaxYears.Get()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private methods
        private void validate(TaxSetup _ts)
        {
            if (_ts.PFThreshold < 0)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixZero("PF Threshold"));

            if (_ts.Details.Count == 0)
            {
                ModelState.AddModelError("Validation", "=> Atleast one Salary slab must exist in Tax Setup to perform save operation");
            }
            else
            {
                if (_ts.Details.Where(x => x.IncomeFrom < 0).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixZero("Income From"));

                if (_ts.Details.Where(x => x.IncomeTo < 0).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixZero("Income To"));

                if (_ts.Details.Where(x => x.BaseTax < 0).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixZero("Base Tax"));

                if (_ts.Details.Where(x => x.TaxRate < 0).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.SuffixZero("Tax Rate"));

                if (_ts.Details.Where(x => x.IncomeFrom > x.IncomeTo).Any())
                    ModelState.AddModelError("Validation", AgilityHelper.Suffixgt("Income From", "Income To"));
            }
        }
        #endregion
    }
}
