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
    public class GradeController : Controller
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
                return Json(new { success = true, Grade = new Grade(), formMode = AgilityEnums.formMode_Add }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult Get(short _keyId)
        {
            var msg = new Object(); 
            try
            {
                Grade _g = Grade.Get(_keyId);
                if (_g == null)
                {
                    throw new Exception("The Grade Id entered doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, Grade = _g, formMode = AgilityEnums.formMode_ReadOnly };
            }
            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Recall };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Save(Grade _g)
        {
            var msg = new Object();
            try
            {
                if ((!_g.GradeId.HasValue && ServerSession.AddAllowed(optionName)) ||
                      (_g.GradeId.HasValue && ServerSession.EditAllowed(optionName)))
                {
                    #region Validation
                    validate(_g);
                    #endregion

                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (Grade.Save(_g))
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
        public JsonResult Search()
        {
            try
            {
                return Json(new
                {
                    success = true,
                    columnDefs = ColumnDefinition.getColDef(new Grades()),
                    rowData = Grades.Get(false)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private methods
        private void validate(Grade _g)
        {
            if (string.IsNullOrEmpty(_g.GradeNo))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Grade #"));

            if (string.IsNullOrEmpty(_g.GradeName))
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Grade Name"));
        }
        #endregion
    }
}



