using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Master.Models
{
    public class LeaveTypeList : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order =0, Name = "Leave Type Id")]
        public short LeaveTypeId { get; set; }
        [DataMember(Order =1, Name = "Leave Type Name")]
        public string LeaveTypeName { get; set; }
        [DataMember(Order =2, Name ="Is Active?")]
        public bool IsActive { get; set; }
        #endregion

        #region constructor
        public LeaveTypeList()
        {
        }

        public LeaveTypeList(short _leaveTypeId, string _leaveTypeName, bool _isActive = true)
        {
            LeaveTypeId = _leaveTypeId;
            LeaveTypeName = _leaveTypeName;
            IsActive = _isActive;
        }
        #endregion

        #region internal methods
        internal static List<LeaveTypeList> Get(bool _isActive)
        {
            List<LeaveTypeList> lstLT = new List<LeaveTypeList>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetLeaveTypes"))
            {
                db.AddInParameter(dbCommand, "ActiveOnly", SqlDbType.Bit, _isActive);
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            lstLT.Add(new LeaveTypeList(
                               Convert.ToInt16(dr["LeaveTypeId"]),
                                dr["LeaveTypeName"].ToString(),
                                 Convert.ToBoolean(dr["IsActive"])));
                        }
                    }
                }
            }
            return lstLT;
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