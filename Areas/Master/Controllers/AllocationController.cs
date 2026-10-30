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
    public class AllocationController : Controller
    {
        private static string optionName = "";
        #region public methods
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
                return Json(new { success = true, allocation = new Allocation(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(Allocation _at)
        {
            var msg = new Object();
            try
            {
                if ((!_at.TemplateId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_at.TemplateId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region Validation
                    validate(_at);
                    #endregion

                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (Allocation.Save(_at))
                        {
                            msg = new { success = true, message = AgilityEnums.Save_Success, formMode = AgilityEnums.formMode_Initialize };
                        }
                    }
                    if (!ModelState.IsValid)
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
                Allocation _at = Allocation.Get(_keyId);
                if (_at == null)
                {
                    throw new Exception("The Allocation Template Id entered doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, allocation= _at, formMode = AgilityEnums.formMode_ReadOnly };
            }
            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Recall };
                return Json(msg, JsonRequestBehavior.AllowGet);
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
                    columnDefs = ColumnDefinition.getColDef(new Allocations()),
                    rowData = Allocations.Get(false)
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
                return Json(new { lstDepartment = Departments.Get()}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private methods
        private void validate(Allocation _at)
        {
            if (string.IsNullOrEmpty(_at.TemplateName))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Template Name"));

            if (_at.Details.Count == 0)
            {
                ModelState.AddModelError("Validation", "=> Atleast one Allocation must exist in Template to perform save operation");
            }

            else if (_at.Details.Where(x => string.IsNullOrEmpty(x.DepartmentCode)).Any())
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Department"));

            else if (_at.Details.Where(x => !x.Delete).GroupBy(x => x.DepartmentCode).Any(grp => grp.Count() > 1))
                ModelState.AddModelError("Validation", "=> Department used in Template must be unique");

            else if (_at.Details.Where(x => x.Rate <= 0).Any())
                ModelState.AddModelError("Validation", AgilityHelper.SuffixgtZero("Rate"));

            if (_at.Details.Where(x => !x.Delete).Sum(x => x.Rate) != 100)
                ModelState.AddModelError("Validation", "=> The Department wise Allocation total must be 100%");
        }
        #endregion
    }
}



