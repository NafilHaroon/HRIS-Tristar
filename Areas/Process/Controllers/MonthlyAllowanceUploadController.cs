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
    public class MonthlyAllowanceUploadController : Controller
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
                    List<MonthlyAllowance> lstMA = uploadData(files);
                    msg = new { success = true, mau = lstMA, formMode = AgilityEnums.formMode_Initialize };
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
            return new FilePathResult(@"~\Content\template\MonthlyAllowanceUpload.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
        #endregion

        #region private methods
        private List<MonthlyAllowance> uploadData(HttpPostedFileBase[] _file)
        {
            List<MonthlyAllowance> _lstMA = new List<MonthlyAllowance>();
            byte[] fileBytes = new byte[_file[0].ContentLength];
            var data = _file[0].InputStream.Read(fileBytes, 0, _file[0].ContentLength);
            MemoryStream stream = new MemoryStream(fileBytes);
            IExcelDataReader xlReader = _file[0].ContentType.Contains("openxmlformats") ? ExcelReaderFactory.CreateOpenXmlReader(stream) : ExcelReaderFactory.CreateBinaryReader(stream);
            xlReader.IsFirstRowAsColumnNames = true;
            DataSet dsXL = xlReader.AsDataSet();

            if (dsXL.Tables.Count == 0)
                throw new Exception("The Selected file doesn't contain any worksheet");
            else if (dsXL.Tables.IndexOf("MonthlyAllowance") < 0)
                throw new Exception("The Selected file doesn't contain any worksheet with the name 'MonthlyAllowance'");

            DataTable dtMonthlyAllowance = dsXL.Tables["MonthlyAllowance"];

            if (!dtMonthlyAllowance.Columns.Contains("EmployeeCode"))
                throw new Exception("EmployeeCode column is missing from the uploaded file");
            else if (!dtMonthlyAllowance.Columns.Contains("HeadId"))
                throw new Exception("HeadId column is missing from the uploaded file");
            else if (!dtMonthlyAllowance.Columns.Contains("Remarks"))
                throw new Exception("Remarks column is missing from the uploaded file");
            else if (!dtMonthlyAllowance.Columns.Contains("Amount"))
                throw new Exception("Amount column is missing from the uploaded file");

            int _row = 1;
            string _employeeCode, _remarks ; object _headId, _amount;
            foreach (DataRow dr in dtMonthlyAllowance.Rows)
            {
                _employeeCode = dr["EmployeeCode"].ToString();
                _headId = dr["HeadId"];
                _amount = dr["Amount"];
                _remarks = dr["Remarks"].ToString();
                try
                {
                    if (string.IsNullOrWhiteSpace(_employeeCode))
                        _lstMA.Add(new MonthlyAllowance(" ", null, 0, "", "Employee Code is missing on row # " + _row.ToString()));
                    else if (dr["HeadId"] == DBNull.Value)
                        _lstMA.Add(new MonthlyAllowance(_employeeCode, null, 0, "", "Head Id is missing"));
                    else if (_amount == DBNull.Value || Convert.ToDecimal(_amount) == 0)
                        _lstMA.Add(new MonthlyAllowance(_employeeCode, Convert.ToInt16(_headId), 0, "", "Amount is missing"));
                    else
                    {
                        MonthlyAllowance.Upload(new MonthlyAllowance(_employeeCode, Convert.ToInt16(_headId), Convert.ToDecimal(_amount), _remarks));
                        _lstMA.Add(new MonthlyAllowance(_employeeCode, Convert.ToInt16(_headId), Convert.ToDecimal(_amount), _remarks));
                    }
                }
                catch (Exception ex)
                {
                    _lstMA.Add(new MonthlyAllowance(_employeeCode, Convert.ToInt16(_headId), Convert.ToDecimal(_amount), _remarks, ex.Message));
                }
                _row++;
            }
            return _lstMA;
        }

        //private ActionResult setAccessOption()
        //{
        //    DataRow[] dr = ((DataTable)Session["AccessOption"]).Select("OptionName = 'Monthly Allowances Upload'");
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