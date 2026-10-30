using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Process.Models
{
    public class LeaveUpload: IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string EmployeeCode { get; set; }
        public DateTime? LeaveDate { get; set; }
        public DateTime? LeaveTime { get; set; }
        public string Status { get; set; }
        public string UploadIndicator { get; set; }
        public List<LeaveUpload> Details { get; set; }
        #endregion

        #region constructor
        public LeaveUpload()
        {
            Details = new List<LeaveUpload>();
        }
        public LeaveUpload(string _employeeCode, DateTime _date, DateTime _time,  string _uploadIndicator, string _status)
        {
            EmployeeCode = _employeeCode;
            LeaveDate = _date;
            LeaveTime = _time;
            UploadIndicator=_uploadIndicator;
            Status = _status;
        }
        #endregion

        #region internal methods
        internal static bool Upload(LeaveUpload _leave)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("SaveLeaveUploadData"))
                {
                    db.AddInParameter(dbCommand, "EmployeeCode", SqlDbType.VarChar, _leave.EmployeeCode);
                    db.AddInParameter(dbCommand, "DateFrom", SqlDbType.DateTime, _leave.LeaveDate);
                    db.AddInParameter(dbCommand, "DateTo", SqlDbType.DateTime, _leave.LeaveTime);
                    db.AddInParameter(dbCommand, "UploadIndicator", SqlDbType.VarChar, _leave.UploadIndicator);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.ExecuteNonQuery(dbCommand);
                }
                return true;
            }
            catch (Exception)
            { throw; }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            // no implementation
        }
        #endregion
    }
}