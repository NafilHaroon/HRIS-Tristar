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
    public class LetterTemplateController : Controller
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
                return Json(new { success = true, template = new LetterTemplate(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(LetterTemplate _lt)
        {
            var msg = new Object();
            try
            {
                if ((!_lt.TemplateId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_lt.TemplateId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region validation
                    validate(_lt);
                    #endregion

                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (LetterTemplate.Save(_lt))
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
                LetterTemplate _lt = LetterTemplate.Get(_keyId);
                if (_lt == null)
                {
                    throw new Exception("The Letter Template Id doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, template = _lt, formMode = AgilityEnums.formMode_ReadOnly };
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
                    columnDefs = ColumnDefinition.getColDef(new LetterTemplates()),
                    rowData = LetterTemplates.Get(false)
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
                    lstDocumentType = DocumentTypes.Get()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private methods
        private void validate(LetterTemplate _lt)
        {
            if (string.IsNullOrEmpty(_lt.TemplateName))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Template Name"));

            if (string.IsNullOrEmpty(_lt.LetterPrefix))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Letter Prefix"));

            if (!_lt.DocumentTypeId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Document Type"));

            if (string.IsNullOrEmpty(_lt.TemplateContent))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Template Content"));
            else
            {
                if (_lt.TemplateContent.IndexOf("@@@Letter_No") < 0)
                    ModelState.AddModelError("Validation", "Letter Template must contain literal @@@Letter_No to embed system generated unique letter number while saving Letter");
                if (_lt.ImpactsPayroll && _lt.TemplateContent.IndexOf("@@@Salary_Breakup") < 0)
                    ModelState.AddModelError("Validation", "Letter Template must contain literal @@@Salary_Breakup when `Impacts Payroll` flag is turned-on");
                if (!_lt.IncludeSeparatedEmployees && _lt.TemplateContent.IndexOf("@@SeparationDate") > -1)
                    ModelState.AddModelError("Validation", "Letter Template can only contain literal @@SeparationDate when `Include Separated Employees` flag is turned-on");
            }
        }
        #endregion
    }
}
