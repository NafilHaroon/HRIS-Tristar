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
    public class DepartmentController : Controller
    {
        private static string optionName = "";
        #region public methods
        public ActionResult DataForm(string screenName)
        {
            optionName = screenName;
            return View();
        }

        [HttpPost]
        public JsonResult Save(Department _d)
        {
            var msg = new Object();
            try
            {
                if (ServerSession.EditAllowed(optionName))
                {
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (Department.Save(_d))
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
        public JsonResult Get(string _keyId)
        {
            var msg = new Object();
            try
            {
                Department _d = Department.Get(_keyId);
                if (_d == null)
                {
                    throw new Exception("The Department Code entered doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, department = _d, formMode = AgilityEnums.formMode_ReadOnly };
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
            try
            {
                return Json(new
                {
                    success = true,
                    columnDefs = ColumnDefinition.getColDef(new Departments()),
                    rowData = Departments.Get(false)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetLookups()
        {
            return Json(new
            {
                success = true,
                lstHFMS = HFMSubCategory.Get()
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}