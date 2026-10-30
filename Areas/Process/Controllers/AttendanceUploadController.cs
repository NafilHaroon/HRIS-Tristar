using Excel;
using HRIS.Areas.Common.Models;
using HRIS.Areas.Process.Models.Upload;
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
    public class AttendanceUploadController : Controller
    {
        private static string optionName = "";
        #region public methods
        public ActionResult DataForm(string screenName)
        {
            optionName = screenName;
            return View();
        }

        [HttpPost]
        public JsonResult Upload(HttpPostedFileBase[] files)
        {
            var msg = new Object();
            try
            {
                #region save to db
                if (ModelState.IsValid)
                {
                    List<Attendance> lstATT = uploadData(files);
                    msg = new { success = true, au = lstATT, formMode = AgilityEnums.formMode_Initialize };               
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

        public FileResult DownloadTemplate()
        {
            return new FilePathResult(@"~\Content\template\AttendanceUpload.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
        #endregion

        #region upload function
        private List<Attendance> uploadData(HttpPostedFileBase[] _file)
        {
            List<Attendance> _lstATT = new List<Attendance>();
            byte[] fileBytes = new byte[_file[0].ContentLength];
            var data = _file[0].InputStream.Read(fileBytes, 0, _file[0].ContentLength);
            MemoryStream stream = new MemoryStream(fileBytes);
            IExcelDataReader xlReader = _file[0].ContentType.Contains("openxmlformats") ? ExcelReaderFactory.CreateOpenXmlReader(stream) : ExcelReaderFactory.CreateBinaryReader(stream);
            xlReader.IsFirstRowAsColumnNames = true;
            DataSet dsXL = xlReader.AsDataSet();

            if (dsXL.Tables.Count == 0)
                throw new Exception("The selected file doesn't contain any worksheet");
            else if (dsXL.Tables.IndexOf("Attendance") < 0)
                throw new Exception("The selected file doesn't contain any worksheet with the name 'Attendance'");
            
            DataTable dtAttendance = dsXL.Tables["Attendance"];

            if (!dtAttendance.Columns.Contains("EmployeeCode"))
                throw new Exception("EmployeeCode column is missing from the uploaded file");
            else if (!dtAttendance.Columns.Contains("AttendanceDate"))
                throw new Exception("AttendanceDate column is missing from the uploaded file");
            else if (!dtAttendance.Columns.Contains("AttendanceTime"))
                throw new Exception("AttendanceTime column is missing from the uploaded file");

            int _row = 1;
            string _employeeCode; object _date, _time;
            DateTime _dt, _tm;
            foreach (DataRow dr in dtAttendance.Rows)
            {
                _employeeCode = dr["EmployeeCode"].ToString();
                _date = dr["AttendanceDate"];
                _time = dr["AttendanceTime"];
                try
                {
                    if (string.IsNullOrWhiteSpace(_employeeCode))
                        _lstATT.Add(new Attendance(" ", null, null, "Employee Code is missing on row # " + _row.ToString()));
                    else if (_date == DBNull.Value)
                        _lstATT.Add(new Attendance(dr["EmployeeCode"].ToString(), null, null, "Attendance Date is missing on row # " + _row.ToString()));
                    else if (!DateTime.TryParse(_date.ToString(), out _dt))
                        _lstATT.Add(new Attendance(dr["EmployeeCode"].ToString(), null, null, "Attendance Date is not a valid date on row # " + _row.ToString()));
                    else if (_time == DBNull.Value)
                        _lstATT.Add(new Attendance(dr["EmployeeCode"].ToString(), _date, null, "Attendance Time is missing on row # " + _row.ToString()));
                    else if (!DateTime.TryParse(_time.ToString(), out _tm))
                        _lstATT.Add(new Attendance(dr["EmployeeCode"].ToString(), _date, null, "Attendance Time is not a valid time on row # " + _row.ToString()));
                    else
                    {
                        Attendance.Upload(new Attendance(_employeeCode, _date, _time));
                        _lstATT.Add(new Attendance(_employeeCode, _date, _time));
                    }
                }
                catch (Exception ex)
                {
                    _lstATT.Add(new Attendance(_employeeCode, _date, _time, ex.Message));
                }
                _row++;
            }
            return _lstATT;
        }
        #endregion

        #region private methods
        //private ActionResult setAccessOption()
        //{
        //    DataRow[] dr = ((DataTable)Session["AccessOption"]).Select("OptionName =  'Attendance Upload'");
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