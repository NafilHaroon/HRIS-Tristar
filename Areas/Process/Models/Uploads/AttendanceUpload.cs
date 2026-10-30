using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Process.Models.Upload
{
    public class Attendance: IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string EmployeeCode { get; set; }
        public DateTime? AttendanceDate { get; set; }
        public DateTime? AttendanceTime { get; set; }
        public string Status { get; set; }
        #endregion

        #region constructor
        public Attendance()
        {
            
        }
        public Attendance(string _employeeCode, object _date, object _time, string _status = "Success")
        {
            EmployeeCode = _employeeCode;
            if (_date != DBNull.Value)
                AttendanceDate = Convert.ToDateTime(_date);
            if (_time != DBNull.Value)
                AttendanceTime = Convert.ToDateTime(_time);
            Status = _status;
        }
        #endregion

        #region internal methods
        internal static bool Upload(Attendance _au)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("UploadAttendance"))
                {
                    db.AddInParameter(dbCommand, "EmployeeCode", SqlDbType.VarChar, _au.EmployeeCode);
                    db.AddInParameter(dbCommand, "AttendanceDate", SqlDbType.DateTime, _au.AttendanceDate);
                    db.AddInParameter(dbCommand, "AttendanceTime", SqlDbType.DateTime, _au.AttendanceTime);
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