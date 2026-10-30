using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace HRIS.Areas.Transaction.Models
{
    public class AttendanceDetail : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public DateTime? AttendanceDate { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public bool OfficialVisit { get; set; }
        public bool Edit { get; set; }
        #endregion

        #region constructor
        public AttendanceDetail()
        {
            Edit = false; 
        }

        public AttendanceDetail(DateTime _date, object _timeIn, object _timeOut, bool _officialVisit)
        {
            AttendanceDate = _date;
            if (_timeIn != DBNull.Value)
                TimeIn = Convert.ToDateTime(_timeIn);
            if (_timeOut != DBNull.Value)
                TimeOut = Convert.ToDateTime(_timeOut);
            OfficialVisit = _officialVisit;
            Edit = false; 
        }
        #endregion

        #region internal methods
        internal static List<AttendanceDetail> Get(short _employeeId)
        {
            try
            {
                List<AttendanceDetail> lstAD = new List<AttendanceDetail>();
                using (DbCommand dbCommand = db.GetStoredProcCommand("GetAttendanceByEmployeeId"))
                {
                    db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _employeeId);
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstAD.Add(new AttendanceDetail(
                                    Convert.ToDateTime(dr["AttendanceDate"]),
                                    dr["TimeIn"],
                                    dr["TimeOut"],
                                    Convert.ToBoolean(dr["OfficialVisit"])));
                            }
                        }
                    }
                }
                return lstAD;
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(short _employeeId, List<AttendanceDetail> _details, DbTransaction _transaction)
        {
            try
            {
                string _userId = ServerSession.GetCurrentUser();
                foreach (AttendanceDetail _ad in getAttDetailChanges(_details))
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("SaveAttendance"))
                    {
                        db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _employeeId);
                        db.AddInParameter(dbCommand, "AttendanceDate", SqlDbType.DateTime, _ad.AttendanceDate);
                        db.AddInParameter(dbCommand, "TimeIn", SqlDbType.DateTime, _ad.TimeIn);
                        db.AddInParameter(dbCommand, "TimeOut", SqlDbType.DateTime, _ad.TimeOut);
                        db.AddInParameter(dbCommand, "OfficialVisit", SqlDbType.Bit, _ad.OfficialVisit);
                        db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, _userId);
                        db.ExecuteNonQuery(dbCommand, _transaction);
                    }
                }
                return true;
            }
            catch (Exception)
            { throw; }
        }
        #endregion

        #region private methods
        private static IEnumerable<AttendanceDetail> getAttDetailChanges(List<AttendanceDetail> _details)
        {
            return _details.Where(x => x.Edit);
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