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
    public class LeaveUploadController : Controller
    {
        private static string optionName = "";
        #region public methods
        public ActionResult DataForm(string screenName)
        {
            optionName = screenName;
            return View();
        }

        [HttpPost]
        public JsonResult Save(DateTime FirstDay, HttpPostedFileBase[] files)
        {
            var msg = new Object();
            LeaveUpload _lu = new LeaveUpload();
            try
            {
                if (ServerSession.AddAllowed(optionName))
                {
                    #region save to db
                    if (ModelState.IsValid)
                    {
                        ProcessUploadLeave(files, FirstDay, out _lu);
                        msg = new { success = true, LeaveUpload = _lu, formMode = AgilityEnums.formMode_Initialize };
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
                msg = new { success = false, LeaveUpload = _lu, message = ex.Message, formMode = AgilityEnums.formMode_Edit };
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region upload function
        private bool ProcessUploadLeave(HttpPostedFileBase[] file, DateTime _DFirstDay, out LeaveUpload lstlu)
        {
            lstlu = new LeaveUpload();
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
            else if (dsXL.Tables.IndexOf("Leave") < 0)
                throw new Exception("The selected file doesn't contain any worksheet with the name 'Leave'");


            DataTable dtLeave = dsXL.Tables["Leave"];

            if (dtLeave.Columns.IndexOf("EmployeeCode") < 0)
                throw new Exception("Employee Code column was not found under 'Leave' worksheet");

            string strMissingCol = ""; bool blnDayColumnThere = false;

            #region validate if all columns are available
            DateTime firstDate = _DFirstDay;
            DateTime[] LeaveDays = new DateTime[31];
            int intCntr = 0;
            foreach (DataColumn dc in dtLeave.Columns)
            {
                if (dc.ColumnName.StartsWith("Day") || dc.ColumnName.ToString() == "EmployeeCode")
                {
                    if (dc.ColumnName.StartsWith("Day"))
                    {
                        blnDayColumnThere = true;
                        LeaveDays[intCntr++] = firstDate;
                        firstDate = firstDate.AddDays(1);
                    }
                }
            }

            if (!blnDayColumnThere)
            {
                strMissingCol += ", Day columns";
            }
            if (strMissingCol.Length > 0)
            {
                throw new Exception("Following column headers are missing from Excel sheet you have selected for upload. " +
                       "The system has abandoned the upload process. Please fix the issue and run upload process once again. Please contact your System Admin if you require any further details\n\n" + strMissingCol);
            }
            #endregion            
            foreach (DataRow dr in dtLeave.Rows)
            {
                DateTime DateFrom = DateTime.Now, DateTo = DateTime.Now;
                try
                {
                    string strUploadIndicator = "";
                    intCntr = 0;
                    foreach (DataColumn dc in dtLeave.Columns)
                    {
                        //DateTime _date = new DateTime();
                        if (dr["EmployeeCode"] == DBNull.Value)
                            lstlu.Details.Add(new LeaveUpload(dr["EmployeeCode"].ToString(), Convert.ToDateTime(dr["LeaveDate"]),
                                Convert.ToDateTime(dr["Leave Time"]), "", "Employee Code Column can't be empty"));
                        //else if (dr["LeaveDate"] == DBNull.Value)
                        //    lstlu.Add(new LeaveUpload(dr["EmployeeCode"].ToString(), Convert.ToDateTime(dr["LeaveDate"]),
                        //        Convert.ToDateTime(dr["Leave Time"]), "","Leave Date column can't be empty"));                
                        //else if (!DateTime.TryParse(dr["LeaveDate"].ToString(), out _date))
                        //    lstlu.Add(new LeaveUpload(dr["EmployeeCode"].ToString(), Convert.ToDateTime(dr["LeaveDate"]),
                        //        Convert.ToDateTime(dr["Leave Time"]), "", "Leave Date must be a valid Date"));

                        else
                        {
                            if (dc.ColumnName.StartsWith("Day"))
                            {
                                if (dr[dc] != DBNull.Value && (dr[dc].ToString() == "C.L"
                                    || dr[dc].ToString() == "S.L"
                                    || dr[dc].ToString() == "A.L"
                                    || dr[dc].ToString() == "H.C.L"
                                    || dr[dc].ToString() == "H.S.L"
                                    || dr[dc].ToString() == "H.M.L"))
                                {
                                    DateFrom = LeaveDays[intCntr];
                                    DateTo = DateFrom;
                                    strUploadIndicator = dr[dc.ColumnName].ToString();

                                    LeaveUpload _au = new LeaveUpload(
                                     dr["EmployeeCode"].ToString(),
                                     DateFrom,
                                     DateTo, dr[dc.ColumnName].ToString(), "");

                                    LeaveUpload.Upload(_au);
                                    lstlu.Details.Add(new LeaveUpload(dr["EmployeeCode"].ToString(), DateFrom,
                                      DateFrom, "", "Success"));
                                }
                                intCntr++;
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    lstlu.Details.Add(new LeaveUpload(dr["EmployeeCode"].ToString(), DateFrom,
                           DateFrom, "", ex.Message));
                }

            }
            return true;
        }
        #endregion


        #region private methods
        //private ActionResult setAccessOption()
        //{
        //    DataRow[] dr = ((DataTable)Session["AccessOption"]).Select("OptionName =  'Upload Leave Data from Excel'");
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

        public FileResult DownloadLUTemplate()
        {
            return new FilePathResult(@"~\Content\template\LeaveUpload.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        #endregion
    }
}