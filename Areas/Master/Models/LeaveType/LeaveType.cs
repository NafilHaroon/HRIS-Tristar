using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class LeaveType : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; }
        public short DefaultDays { get; set; }
        public string UploadIndicator { get; set; }
        public string HDUploadIndicator { get; set; }
        public bool IsActive { get; set; }
        public Footer AuditTrail { get; set; }
        #endregion

        #region constructor
        public LeaveType()
        {
            AuditTrail = new Footer();
            IsActive = true;
        }
        #endregion

        #region internal methods
        internal static LeaveType Get(short _leaveTypeId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetLeaveTypeById"))
            {
                db.AddInParameter(dbCommand, "LeaveTypeId", SqlDbType.SmallInt, _leaveTypeId);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (LeaveType _lv = new LeaveType())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _lv.LeaveTypeId = Convert.ToInt16(dr["LeaveTypeId"]);
                            _lv.LeaveTypeName = dr["LeaveTypeName"].ToString();
                            _lv.DefaultDays = Convert.ToInt16(dr["DefaultDays"]);
                            _lv.UploadIndicator = dr["UploadIndicator"].ToString();
                            _lv.HDUploadIndicator = dr["HDUploadIndicator"].ToString();
                            _lv.IsActive = Convert.ToBoolean(dr["IsActive"]);
                            _lv.AuditTrail.CreatedBy = dr["CreatedBy"].ToString();
                            _lv.AuditTrail.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                            _lv.AuditTrail.UpdatedBy = dr["UpdatedBy"].ToString();
                            if (dr["UpdatedOn"] != DBNull.Value)
                                _lv.AuditTrail.UpdatedOn = Convert.ToDateTime(dr["UpdatedOn"]);
                            return _lv;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(LeaveType _lt)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveLeaveType"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "LeaveTypeId", SqlDbType.TinyInt, _lt.LeaveTypeId);
                    db.AddInParameter(dbCommand, "LeaveTypeName", SqlDbType.VarChar, _lt.LeaveTypeName);
                    db.AddInParameter(dbCommand, "DefaultDays", SqlDbType.TinyInt, _lt.DefaultDays);
                    db.AddInParameter(dbCommand, "UploadIndicator", SqlDbType.VarChar, _lt.UploadIndicator);
                    db.AddInParameter(dbCommand, "HDUploadIndicator", SqlDbType.VarChar, _lt.HDUploadIndicator);
                    db.AddInParameter(dbCommand, "IsActive", SqlDbType.Bit, _lt.IsActive);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _lt.AuditTrail.UpdatedOn);
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
            // no implementation
        }
        #endregion
    }
}
