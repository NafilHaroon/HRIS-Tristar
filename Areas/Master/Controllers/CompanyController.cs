using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using HRIS.Areas.PF.Models;
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
    public class CompanyController : Controller
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
                return Json(new { success = true, company = new Company(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(Company _c)
        {
            var msg = new Object();
            try
            {
                if ((!_c.CompanyId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_c.CompanyId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region Validation
                    validate(_c);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (Company.Save(_c))
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
                Company _c = Company.Get(_keyId);
                if (_c == null)
                {
                    throw new Exception("The Company Id doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, company = _c, formMode = AgilityEnums.formMode_ReadOnly };
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
                    columnDefs = ColumnDefinition.getColDef(new CompanyList()),
                    rowData = CompanyList.Get(false)
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
                    lstPFAccount = CoAList.Get(1),
                    lstBankAccount = CoAList.GetBankAccounts(),
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetUserDefaultCompany()
        {
            try
            {
                CompanyLogo usercompany = Company.GetUserDefaultCompany();
                Session["CompanyId"] = usercompany.CompanyId;
                return Json(new { usercompany }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SetUserDefaultCompany(short _companyId)
        {
            var msg = new Object();
            try
            {
                if (Company.SetUserDefaultCompany(_companyId))
                {
                    #region refresh period as per company
                    Session["CompanyId"] = _companyId;
                    Period _p = Period.GetCurrentPeriod();
                    Session["PeriodId"] = _p.PeriodId;
                    Session["PeriodName"] = _p.PeriodName;
                    #endregion
                    msg = new { success = true, message = "Default company set successfully" };
                }
                else
                {
                    msg = new { success = false, message = "There was an error updating default company selecton for user!" };
                }
            }
            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message};
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUserCompanies()
        {
            return Json(CompanyList.Get(true), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region private methods
        private void validate(Company _c)
        {
            if (string.IsNullOrEmpty(_c.CompanyName))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Company Name"));

            if (string.IsNullOrEmpty(_c.CompanyCode))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Company Code"));

            if (string.IsNullOrEmpty(_c.CompanyAddress))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Company Address"));

            if (string.IsNullOrEmpty(_c.NTN))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("NTN"));

            if (!string.IsNullOrEmpty(_c.NTN))
            {
                if (!Regex.IsMatch(_c.NTN, @"^([0-9]{8})-([0-9]{1})$"))
                    ModelState.AddModelError("Validation", "=> Please enter NTN # in valid format like 17129885-6");
            }

            if (string.IsNullOrEmpty(_c.HFMCode))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("HFM Code"));

            if (string.IsNullOrEmpty(_c.EOBIRegNo))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("EOBI Registration #"));

            if (_c.EOBIContribution <= 0)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixgtZero("EOBI Contribution"));
        }
        #endregion
    }
}
