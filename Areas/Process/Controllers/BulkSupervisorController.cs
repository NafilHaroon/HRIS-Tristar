using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using HRIS.Areas.Process.Models;
using HRIS.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace HRIS.Areas.Process.Controllers
{
    [NoCache]
    [Authorize]
    public class BulkSupervisorController : Controller
    {
        private static string optionName = "";
        #region public methods
        public ActionResult DataForm(string screenName)
        {
            optionName = screenName;
            return View();
        }

        [HttpPost]
        public JsonResult Save(BulkSupervisor _bs)
        {
            var msg = new Object();
            try
            {
                if (ServerSession.EditAllowed(optionName))
                {
                    #region validation
                    validate(_bs);
                    #endregion
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        if (BulkSupervisor.Update(_bs))
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
                    lstBranch = Branches.Get(true),
                    lstDepartment = Departments.Get(),
                    lstSupervisor = EmployeeShortList.GetSupervisors(),
                    lstEmployee = EmployeeShortList.Get()
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
        //private ActionResult setAccessOption()
        //{
        //    DataRow[] dr = ((DataTable)Session["AccessOption"]).Select("OptionName =  'Bulk Supervisor Update'");
        //    if (dr.Count() > 0)
        //    {
        //        Session["AllowAdd"] = Convert.ToBoolean(dr[0]["canAdd"]);
        //        Session["AllowDelete"] = Convert.ToBoolean(dr[0]["canDelete"]);
        //        Session["AllowEdit"] = Convert.ToBoolean(dr[0]["canEdit"]);
        //    }
        //    else
        //    {
        //        ViewBag.Menu = (List<Menu>)Session["MainMenu"];
        //        return View("InvalidMenuOption");
        //    }
        //    return View();
        //}

        private void validate(BulkSupervisor _cs)
        {            
            //if (string.IsNullOrEmpty(_cs.DepartmentCode))
            //    ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("Department"));

            if (!_cs.SupervisorId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF(" Current Supervisor"));

            if (!_cs.NewSupervisorId.HasValue)
                ModelState.AddModelError("Validation", AgilityHelper.SuffixRF("New Supervisor"));

            if (_cs.SupervisorId == _cs.NewSupervisorId)
                ModelState.AddModelError("Validation", "New Supervisor can not be same as Current Supervisor");
        }
        #endregion
    }
}