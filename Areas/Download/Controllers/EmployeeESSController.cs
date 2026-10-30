using HRIS.Areas.Common.Models;
using HRIS.Areas.Download.Models;
using HRIS.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace HRIS.Areas.Download.Controllers
{
    [NoCache]
    [Authorize]
    public class EmployeeESSController : Controller
    {
        #region internal methods
        public ActionResult DataForm()
        {
            return setAccessOption();
        }        

        [HttpGet]
        public JsonResult Get()
        {
            var msg = new Object(); 
            try
            {
                EmployeeESS _ess = EmployeeESS.Get();
                if (_ess == null)
                {
                    throw new Exception("No data exist on selected parameters or you donot have permission to access it");
                }
                msg = new { success = true, empESS = _ess};
            }

            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message};
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region private methods
        private ActionResult setAccessOption()
        {
            DataRow[] dr = ((DataTable)Session["AccessOption"]).Select("OptionName = 'Employee Detail with ESS'");
            if (dr.Count() > 0)
            {
                Session["AllowAdd"] = Convert.ToBoolean(dr[0]["canAdd"]);
                Session["AllowDelete"] = Convert.ToBoolean(dr[0]["canDelete"]);
                Session["AllowEdit"] = Convert.ToBoolean(dr[0]["canEdit"]);
            }
            else
            {
                ViewBag.Menu = (List<Menu>)Session["MainMenu"];
                return View("InvalidMenuOption");
            }
            Session["FormMode"] = AgilityEnums.formMode.Initialize;
            return View();
        }
            #endregion  
    }
}

