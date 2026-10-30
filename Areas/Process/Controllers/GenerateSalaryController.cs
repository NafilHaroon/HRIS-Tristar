using HRIS.Areas.Common.Models;
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
    public class GenerateSalaryController : Controller
    {
        //private static string optionName = "";
        #region public methods
        public ActionResult DataForm(string screenName)
        {
          //  optionName = screenName;
            return View();
        }

        [HttpPost]
        public JsonResult Generate(bool _overwrite)
        {
            var msg = new Object();
            try
            {
                #region process salary to WIP table for current month
                if (ModelState.IsValid)
                {
                    if (SalaryProcess.Generate(_overwrite))
                    {
                        msg = new
                        {
                            success = true,
                            message = "Salary for the Period " + ServerSession.GetCurrentPeriodName().ToString() + " generated successfully!",
                            formMode = AgilityEnums.formMode_Initialize
                        };
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
            catch (Exception ex)
            {
                msg = new { success = false, message = ex.Message, formMode = AgilityEnums.formMode_Edit };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region private methods
        //private ActionResult setAccessOption()
        //{
        //    DataRow[] dr = ((DataTable)Session["AccessOption"]).Select("OptionName = 'Generate Salary'");
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
        #endregion
    }
}