using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using HRIS.Areas.Process.Models;
using HRIS.Helper;
using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace HRIS.Areas.Process.Controllers
{
    [NoCache]
    [Authorize]
    public class EmailSetupController : Controller
    {
        private static string optionName = "";
        #region public methods
        public ActionResult CSEmail()
        {
            Session["WorkFlowTypeId"] = Request.QueryString["WorkFlowTypeId"];
            return RedirectToActionPermanent("DataForm");
        }

        public ActionResult PREmail()
        {
            Session["WorkFlowTypeId"] = Request.QueryString["WorkFlowTypeId"];
            return RedirectToActionPermanent("DataForm");
        }

        public ActionResult DataForm(string screenName)
        {
            optionName = screenName;
            return View();
        }

        [HttpPost]
        public JsonResult Save(EmailSetup _es)
        {
            var msg = new Object();
            try
            {
                if (ServerSession.EditAllowed(optionName))
                {
                    #region Validation
                    validate(_es);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (Convert.ToInt16(Session["WorkFlowTypeId"]) == 1)
                        {
                            EmailSetup.SaveCSE(_es);
                        }
                        else
                        {
                            EmailSetup.SavePRE(_es);
                        }
                        msg = new { success = true, message = AgilityEnums.Save_Success, formMode = AgilityEnums.formMode_Initialize };

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
        public JsonResult Get()
        {
            var msg = new Object(); 
            try
            {
                EmailSetup _e = new  EmailSetup();
                if (Convert.ToInt16(Session["WorkFlowTypeId"]) == 1)
                {
                    _e = EmailSetup.GetCSE();
                }
                else
                {
                    _e = EmailSetup.GetPRE();
                }

                if (_e == null)
                {
                    throw new Exception("The Montly Salary  doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, EmailSetup = _e, formMode = AgilityEnums.formMode_ReadOnly };
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
                    success = true,
                    lstBranch = BranchList.Get(true),
                    lstCompany = CompanyList.Get(true)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        //}
        #endregion

        #region private methods
        private string getOptionName(short id)
        {
            switch (Convert.ToInt16(id))
            {
                case 1:
                    return "Cash Salary Email Setup";
                case 2:
                    return "Probation Reminder Email Setup";
                default:
                    return null;
            }
        }

        private void validate(EmailSetup _es)
        {
            if (_es.Details.Count == 0)
                ModelState.AddModelError("Validation", "=> Atleast one Detail must exist to perform save operation");

            else if (_es.Details.Where(y => string.IsNullOrWhiteSpace(y.BranchCode)).Any())
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Branch"));

            else if (_es.Details.Where(y => !y.CompanyId.HasValue).Any())
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Company"));

            else if (_es.Details.GroupBy(x => new { x.BranchCode, x.CompanyId }).Any(grp => grp.Count() > 1))
                ModelState.AddModelError("Validation", "=> Branch used must be unique");


            else if (_es.Details.Where(y => string.IsNullOrWhiteSpace(y.ContactName)).Any())
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Contact Name"));

            else if (_es.Details.Where(y => string.IsNullOrWhiteSpace(y.EmailTo)).Any())
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("EmailTo"));
        }
        #endregion
    }
}