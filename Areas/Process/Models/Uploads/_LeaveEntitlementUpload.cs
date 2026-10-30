using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Process.Models
{
    public class LeaveEntitlement: IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string EmployeeCode { get; set; }
        public DateTime? DueDate { get; set; }
        public short? LeaveTypeId { get; set; }
        public short? AllowedDays { get; set; }
        public string Status { get; set; }
        public List<LeaveEntitlement> Details { get; set; }
        #endregion

        #region constructor
        public LeaveEntitlement()
        {
            Details = new List<LeaveEntitlement>();
        }
        public LeaveEntitlement(string _employeeCode, DateTime _dueDate,  short _leaveTypeId, 
            short _allowedDays, string _status)
        {
            EmployeeCode = _employeeCode;
            DueDate = _dueDate;
            LeaveTypeId = _leaveTypeId;
            AllowedDays=_allowedDays;
            Status = _status;
        }
        #endregion

        #region internal methods

        internal static bool Upload(LeaveEntitlement _le)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("SaveLeaveEntitlementUploadData"))
                {
                    db.AddInParameter(dbCommand, "EmployeeCode", SqlDbType.VarChar, _le.EmployeeCode);
                    db.AddInParameter(dbCommand, "DueDate", SqlDbType.DateTime, _le.DueDate);
                    db.AddInParameter(dbCommand, "LeaveTypeId", SqlDbType.SmallInt, _le.LeaveTypeId);
                    db.AddInParameter(dbCommand, "AllowedDays", SqlDbType.SmallInt, _le.AllowedDays);
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