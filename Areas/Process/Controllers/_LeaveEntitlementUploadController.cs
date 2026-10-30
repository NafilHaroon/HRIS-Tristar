using Excel;
using HRIS.Areas.Common.Models;
using HRIS.Areas.Process.Models;
using HRIS.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HRIS.Areas.Process.Controllers
{
    [NoCache]
    [Authorize]
    public class LeaveEntitlementUploadController : Controller
    {
        private static string optionName = "";
        #region public methods
        public ActionResult DataForm(string screenName)
        {
            optionName = screenName;
            return View();
        }

        [HttpPost]
        public JsonResult Save(HttpPostedFileBase[] files)
        {
            var msg = new Object();
            LeaveEntitlement _le = new LeaveEntitlement();
            try
            {
                if (ServerSession.AddAllowed(optionName))
                {
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        ProcessUploadAttendace(files, out _le);
                        msg = new { success = true, LeaveEntitlementUpload = _le, formMode = AgilityEnums.formMode_Initialize };
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
                msg = new { success = false, LeaveEntitlement = _le, message = ex.Message, formMode = AgilityEnums.formMode_Edit };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region upload function
        private bool ProcessUploadAttendace(HttpPostedFileBase[] file, out LeaveEntitlement lstau)
        {
            lstau = new LeaveEntitlement();
            byte[] fileBytes = new byte[file[0].ContentLength];
            var data = file[0].InputStream.Read(fileBytes, 0, file[0].ContentLength);
            MemoryStream stream = new MemoryStream(fileBytes);
            IExcelDataReader xlReader = null;
            bool isXlsx = file[0].ContentType.Contains("openxmlformats");
            xlReader = isXlsx ? ExcelReaderFactory.CreateOpenXmlReader(stream) : ExcelReaderFactory.CreateBinaryReader(stream);
            xlReader.IsFirstRowAsColumnNames = true;
            DataSet dsXL = xlReader.AsDataSet();

            if (dsXL.Tables.Count == 0)
                throw new Exception("The selected file doesn't contain any worksheet");
            else if (dsXL.Tables.IndexOf("LeaveEntitlement") < 0)
                throw new Exception("The selected file doesn't contain any worksheet with the name 'LeaveEntitlement'");


            DataTable dtLeaveEntitlement = dsXL.Tables["LeaveEntitlement"];

            if (dtLeaveEntitlement.Columns.IndexOf("EmployeeCode") < 0)
                throw new Exception("Employee Code column was not found under 'LeaveEntitlement' worksheet");
            else if (dtLeaveEntitlement.Columns.IndexOf("LeaveId") < 0)
                throw new Exception("Leave Id column was not found under 'LeaveEntitlement' worksheet");
            else if (dtLeaveEntitlement.Columns.IndexOf("AllowedDays") < 0)
                throw new Exception("Allowed Days column was not found under 'LeaveEntitlement' worksheet");
            else if (dtLeaveEntitlement.Columns.IndexOf("DueDate") < 0)
                throw new Exception("Due Date column was not found under 'LeaveEntitlement' worksheet");




            foreach (DataRow dr in dtLeaveEntitlement.Rows)
            {
                try
                {
                    DateTime _date = new DateTime();
                    DateTime dtDueDate = Convert.ToDateTime(dr["DueDate"]);
                    if (dr["EmployeeCode"] == DBNull.Value)
                        lstau.Details.Add(new LeaveEntitlement(dr["EmployeeCode"].ToString(), Convert.ToDateTime(dtDueDate.ToString("dd-MMM-yy")),
                         0, 0, "Employee Code Column can't be empty"));
                    else if (dr["DueDate"] == DBNull.Value)
                        lstau.Details.Add(new LeaveEntitlement(dr["EmployeeCode"].ToString(), Convert.ToDateTime(dtDueDate.ToString("dd-MMM-yy")),
                         0, 0, "LeaveEntitlement Date column can't be empty"));
                    else if (dr["LeaveId"] == DBNull.Value)
                        lstau.Details.Add(new LeaveEntitlement(dr["EmployeeCode"].ToString(), Convert.ToDateTime(dtDueDate.ToString("dd-MMM-yy")),
                         0, 0, "Leave Id column can't be empty"));
                    else if (dr["AllowedDays"] == DBNull.Value)
                        lstau.Details.Add(new LeaveEntitlement(dr["EmployeeCode"].ToString(), Convert.ToDateTime(dtDueDate.ToString("dd-MMM-yy")),
                         0, 0, "Allowed Days column can't be empty"));
                    else if (!DateTime.TryParse(dr["DueDate"].ToString(), out _date))
                        lstau.Details.Add(new LeaveEntitlement(dr["EmployeeCode"].ToString(), Convert.ToDateTime(dtDueDate.ToString("dd-MMM-yy")),
                         0, 0, "Due Date must be a valid Date"));

                    else
                    {
                        LeaveEntitlement _au = new LeaveEntitlement(
                        dr["EmployeeCode"].ToString(),
                        Convert.ToDateTime(dr["DueDate"]),
                        Convert.ToInt16(dr["LeaveId"]),
                        Convert.ToInt16(dr["AllowedDays"]), "");

                        LeaveEntitlement.Upload(_au);
                        lstau.Details.Add(new LeaveEntitlement(dr["EmployeeCode"].ToString(), Convert.ToDateTime(dr["DueDate"]),
                            Convert.ToInt16(dr["LeaveId"]), Convert.ToInt16(dr["AllowedDays"]), "Success"));
                    }
                }
                catch (Exception ex)
                {
                    lstau.Details.Add(new LeaveEntitlement(dr["EmployeeCode"].ToString(), Convert.ToDateTime(dr["DueDate"]),
                            Convert.ToInt16(dr["LeaveId"]), Convert.ToInt16(dr["AllowedDays"]), ex.Message));
                }

            }
            return true;
        }
        #endregion

        #region private methods
        //private ActionResult setAccessOption()
        //{
        //    DataRow[] dr = ((DataTable)Session["AccessOption"]).Select("OptionName = 'Upload Leave Entitlement from Excel'");
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

        public FileResult DownloadLEUTemplate()
        {
            return new FilePathResult(@"~\Content\template\LeaveEntitlementUpload.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        #endregion
    }
}