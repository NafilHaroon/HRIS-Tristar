using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using HRIS.Helper;
using System;
using System.Web.Mvc;
using System.Web.Security;

namespace HRIS.Areas.Common.Controllers
{
    [NoCache]
    public class UserController : Controller
    {
        public ActionResult LogIn()
        {
            return View();
        }

        public JsonResult ValidateUser(UserProfile u)
        {
            try
            {
                if (ModelState.IsValid) // this is check validity
                {
                    UserProfile user = new UserProfile
                    {
                        UserId = u.UserId.ToLower(),
                        Password = u.Password
                    };
                    AgilityEnums.UserValidation uv = user.AuthenticateUser();

                    switch (uv)
                    {
                        case AgilityEnums.UserValidation.InvalidUserId:
                            throw new Exception("The user id you have entered is invalid");
                        case AgilityEnums.UserValidation.InvalidPassword:
                            throw new Exception("The Password you have entered is invalid");
                        case AgilityEnums.UserValidation.UserIdDisabled:
                            throw new Exception("The user id you have entered has been disabled. Please contact your System Admin for further details");
                        case AgilityEnums.UserValidation.ForcePasswordChange:
                            Session["UserId"] = u.UserId.ToLower();
                            Session["Password"] = u.Password;
                            return Json(new { success = true, url = Url.Action("ChangePassword", "User") }, JsonRequestBehavior.AllowGet);
                        case AgilityEnums.UserValidation.Successful:
                            Session["UserId"] = user.UserId.ToLower();
                            Session["UserRoleId"] = user.UserRoleId.ToString();
                            Session["UserName"] = user.UserName;
                            Session["AccessOption"] = UserProfile.GetAccess();
                            Session["CompanyId"] = Company.GetUserDefaultCompanyWithoutLogo();
                            Session["MainMenu"] = Menu.Get();
                            Config con = Config.Get();
                            Session["AbsentDeductionId"] = con.AbsentHeadId;
                            Session["BasicSalaryId"] = con.BasicSalaryHeadId;
                            Period _p = Period.GetCurrentPeriod();
                            Session["PeriodId"] = _p.PeriodId;
                            Session["PeriodName"] = _p.PeriodName;
                            FormsAuthentication.SetAuthCookie(user.UserId, false);
                            string returnURL = Request.UrlReferrer.ToString();
                            if (returnURL.IndexOf("ReturnUrl=") > -1)
                            {
                                returnURL = returnURL.Substring(returnURL.IndexOf("ReturnUrl=") + 10);
                                return Json(new { success = true, url = returnURL }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { success = true, url = Url.Action("Index", "Common") }, JsonRequestBehavior.AllowGet);
                            }
                    }
                }
                return Json(new { success = false, url = Url.Action("Login", "User") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            { return Json(new { success = false, errorMsg = ex.Message }, JsonRequestBehavior.AllowGet); }
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            Session.Clear();
            return RedirectToAction("LogIn", "User");
        }

        [HttpGet]
        [AllowAnonymous]
        public EmptyResult SendPassword(string _userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_userId))
                    throw new Exception("Please enter valid user id and then press forgot password to receive your password on email");
                else
                    UserProfile.SendPassword(_userId);
                return new EmptyResult();
            }
            catch (Exception)
            { throw; }
        }

        public JsonResult ApplyNewPassword(ChangePassword _cp)
        {
            try
            {
                if (!AgilityHelper.isComplex(_cp.NewPassword))
                {
                    throw new Exception("Password must contain combination of Capital & Small Alphabet, Number and Special Character " +
                   "e.g. 20-Dec-79 or AqT-607 Description of Failure: Your Password doesn't contain Number (Valid values are 0-9)" +
                   "Capital Alphabet (Valid values are A-Z) Small Alphabet (Valid values are a-z)" +
                   "Special characters (Valid values are any character other than Alphabet and Number)" + "Please try new password using above mentioned criteria" + "," + "Weak Password");
                }
                else
                {
                    try
                    {
                        UserProfile.ChangePassword(Session["UserId"].ToString(), AgilityHelper.MaskPassword(_cp.OldPassword, true), 
                            AgilityHelper.MaskPassword(_cp.NewPassword, true));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                return Json(new { success = true, message = "New password applied successfully"}, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
