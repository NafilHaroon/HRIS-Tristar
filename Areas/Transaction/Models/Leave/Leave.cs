using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Transaction.Models
{
    public class Leave : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int DetailId { get; set; }
        public short? EmployeeId { get; set; }
        public short? LeaveTypeId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool HalfDay { get; set; } = false;
        public bool Uploaded { get; set; } = false;
        public string Reason { get; set; }
        public Footer AuditTrail { get; set; }
        #endregion

        #region constructor
        public Leave()
        {
            AuditTrail = new Footer();
        }

        public Leave(short _employeeId)
        {
            EmployeeId = _employeeId;
        }
        #endregion

        #region internal methods
        internal static Leave Get(short _detailId)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetLeaveById"))
                {
                    db.AddInParameter(dbCommand, "DetailId", SqlDbType.SmallInt, _detailId);
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                        {
                            using (Leave _ml = new Leave())
                            {
                                DataRow dr = ds.Tables[0].Rows[0];
                                _ml.DetailId = _detailId;
                                _ml.EmployeeId = Convert.ToInt16(dr["EmployeeId"]);
                                _ml.LeaveTypeId = Convert.ToInt16(dr["LeaveTypeId"]);
                                _ml.DateFrom = Convert.ToDateTime(dr["DateFrom"]);
                                _ml.DateTo = Convert.ToDateTime(dr["DateTo"]);
                                _ml.HalfDay = Convert.ToBoolean(dr["HalfDay"]);
                                _ml.Uploaded = Convert.ToBoolean(dr["Uploaded"]);
                                _ml.Reason = dr["Reason"].ToString();
                                _ml.AuditTrail.CreatedBy = dr["CreatedBy"].ToString();
                                _ml.AuditTrail.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                                return _ml;
                            }
                        }
                        else
                            return null;
                    }
                }
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(Leave _lv)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveLeave"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _lv.EmployeeId);
                    db.AddInParameter(dbCommand, "LeaveTypeId", SqlDbType.SmallInt, _lv.LeaveTypeId);
                    db.AddInParameter(dbCommand, "DateFrom", SqlDbType.DateTime, _lv.DateFrom);
                    db.AddInParameter(dbCommand, "DateTo", SqlDbType.DateTime, _lv.DateTo);
                    db.AddInParameter(dbCommand, "HalfDay", SqlDbType.Bit, _lv.HalfDay);
                    db.AddInParameter(dbCommand, "Reason", SqlDbType.VarChar, _lv.Reason);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.ExecuteNonQuery(dbCommand);
                    return true;
                }
                catch (Exception)
                { throw; }
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            //
        }
        #endregion
    }
}