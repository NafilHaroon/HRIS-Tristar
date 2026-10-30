using HRIS.Helper;
using System;
using System.Data;
using System.Linq;
using System.Web;

namespace HRIS.Areas.Common.Models
{
    public class ServerSession
    {
        internal static string GetCurrentUser()
        {
            if (HttpContext.Current.Session["UserId"] == null)
                throw new Exception("Current User: Session Expired");
            else
                return HttpContext.Current.Session["UserId"].ToString();
        }

        internal static short GetCurrentCompany()
        {
            if (HttpContext.Current.Session["CompanyId"] == null)
                throw new Exception("Current Company: Session Expired");
            else
                return Convert.ToInt16(HttpContext.Current.Session["CompanyId"]);
        }

        internal static short GetCurrentPeriod()
        {
            if (HttpContext.Current.Session["PeriodId"] == null)
                throw new Exception("Current Period: Session Expired");
            else
                return Convert.ToInt16(HttpContext.Current.Session["PeriodId"]);
        }

        internal static string GetCurrentPeriodName()
        {
            if (HttpContext.Current.Session["PeriodName"] == null)
                throw new Exception("Current Period Name: Session Expired");
            else
                return HttpContext.Current.Session["PeriodName"].ToString();
        }

        internal static short GetPrimaryRole()
        {
            if (HttpContext.Current.Session["UserRoleId"] == null)
                throw new Exception("Primary Role: Session Expired");
            else
                return Convert.ToInt16(HttpContext.Current.Session["UserRoleId"]);
        }

        internal static short GetAbsentId()
        {
            if (HttpContext.Current.Session["AbsentDeductionId"] == null)
                throw new Exception("Absent Id: Session Expired");
            else
                return Convert.ToInt16(HttpContext.Current.Session["AbsentDeductionId"]);
        }

        internal static AccessRights GetAccessRights(string _optionName, bool _includeFormMode)
        {
            if (HttpContext.Current.Session["AccessOption"] == null)
            {
                throw new Exception("Access Option: Session Expired");
            }
            else
            {
                DataRow[] dr = ((DataTable)HttpContext.Current.Session["AccessOption"]).Select("OptionName = '" + _optionName + "'");
                if (dr.Count() > 0)
                {
                    return new AccessRights(dr[0],
                        _includeFormMode ? HttpContext.Current.Session["FormMode"].ToString() : "None");
                }
                else
                {
                    return new AccessRights();
                }
            }
        }

        internal static bool AddAllowed(string _optionName)
        {
            if (HttpContext.Current.Session["AccessOption"] == null)
                throw new Exception("Validate Add Permission: Session Expired");
            else
                return Convert.ToBoolean(((DataTable)HttpContext.Current.Session["AccessOption"]).Select("OptionName = '" + _optionName + "'")[0]["CanAdd"] ?? false);
        }

        internal static bool EditAllowed(string _optionName)
        {
            if (HttpContext.Current.Session["AccessOption"] == null)
                throw new Exception("Validate Edit Permission: Session Expired");
            else
                return Convert.ToBoolean(((DataTable)HttpContext.Current.Session["AccessOption"]).Select("OptionName = '" + _optionName + "'")[0]["CanEdit"] ?? false);
        }

        internal static bool DeleteAllowed(string _optionName)
        {
            if (HttpContext.Current.Session["AccessOption"] == null)
                throw new Exception("Validate Delete Permission: Session Expired");
            else
                return Convert.ToBoolean(((DataTable)HttpContext.Current.Session["AccessOption"]).Select("OptionName = '" + _optionName + "'")[0]["CanDelete"] ?? false);
        }
    }
}
