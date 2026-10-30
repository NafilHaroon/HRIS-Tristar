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
    public class QualificationController : Controller
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
                return Json(new { success = true, qualification = new Qualification(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(Qualification _qual)
        {
            var msg = new Object();
            try
            {
                if ((!_qual.QualificationId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_qual.QualificationId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region Validation
                    validate(_qual);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (Qualification.Save(_qual))
                        {
                            msg = new { success = true, message = AgilityEnums.Save_Success, formMode = AgilityEnums.formMode_Initialize
                            };
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
        public JsonResult Get(short _keyId)
        {
            var msg = new Object();
            try
            {
                Qualification _qual = Qualification.Get(_keyId);
                if (_qual == null)
                {
                    throw new Exception("The Qualification Id doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, qualification = _qual, formMode = AgilityEnums.formMode_ReadOnly };
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
                    columnDefs = ColumnDefinition.getColDef(new Qualifications()),
                    rowData = Qualifications.Get()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private methods
        private void validate(Qualification _qual)
        {
            if (string.IsNullOrEmpty(_qual.QualificationName))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Qualification Name"));
        }
        #endregion
    }
}
