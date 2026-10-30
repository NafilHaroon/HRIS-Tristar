using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace HRIS.Areas.Transaction.Models
{
    public class Attendance : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? EmployeeId { get; set; }
        public short? PeriodId { get; set; }
        public string PeriodName { get; set; }
        public List<AttendanceDetail> Details { get; set; }
        #endregion

        #region constructor
        public Attendance()
        {
            Details = new List<AttendanceDetail>();
            PeriodId = ServerSession.GetCurrentPeriod();
            PeriodName = ServerSession.GetCurrentPeriodName();
        }
        #endregion

        #region internal methods
        internal static Attendance Get(short _employeeId)//, short _periodId)
        {
            try
            {
                using (Attendance _att = new Attendance())
                {
                    _att.EmployeeId = _employeeId;
                    _att.Details = AttendanceDetail.Get(_employeeId);//, _periodId);
                    return _att;
                }
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(Attendance _att)
        {
            using (DbConnection dbconnection = db.CreateConnection())
            {
                dbconnection.Open();
                DbTransaction transaction = dbconnection.BeginTransaction();
                try
                {
                    AttendanceDetail.Save(_att.EmployeeId.Value, _att.Details, transaction);
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
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