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
    public class UserProfileController : Controller
    {
        private static string optionName = "";
        #region public methods
        public ActionResult DataForm(string screenName)
        {
            optionName = screenName;
            return View();
        }

        [HttpPost]
        public JsonResult Save(UserProfile _up)
        {
            var msg = new Object();
            try
            {
                if (ServerSession.EditAllowed(optionName))
                {
                    #region Validationfvd
                    validate(_up);
                #endregion

                #region save to db
                if (ModelState.IsValid)
                {
                    if (UserProfile.Save(_up))
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
                UserProfile _up = UserProfile.Get(_keyId);
                if (_up == null)
                {
                    throw new Exception("The User Id entered doesn't exist or you donot have permission to access it");
                }
                msg = new { success = true, UserProfile = _up, formMode = AgilityEnums.formMode_ReadOnly };
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
                    columnDefs = ColumnDefinition.getColDef(new UserList()),
                    rowData = UserList.Get(false)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region private methods
        private void validate(UserProfile _up)
        {
            if (_up.Options.Where(x => !x.AllowAccess && (x.AllowAdd || x.AllowEdit)).Any())
                ModelState.AddModelError("Validation", "=> Please put check on Allow access when you request Allow Add or Allow Edit else remove check from Allow Add / Edit where Allow access is not turned on");
        }
        #endregion
    }
}