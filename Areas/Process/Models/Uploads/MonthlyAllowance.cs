using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Process.Models.Upload
{
    public class MonthlyAllowance : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string EmployeeCode { get; set; }
        public short? HeadId { get; set; }
        public decimal? Amount { get; set; }
        public string  Remarks { get; set; }       
        public string Status { get; set; }
        //public List<MonthlyAllowance> Details { get; set; }
        #endregion

        #region constructor
        public MonthlyAllowance()
        {
            //Details = new List<MonthlyAllowance>();
        }
         public MonthlyAllowance(string _employeeCode, short? _headId, decimal? _amount, string _remarks, string _status = "success")
        {
            EmployeeCode = _employeeCode;
            HeadId = _headId;
            Amount = _amount;
            Remarks = _remarks;
            Status = _status;
        }
        #endregion

        #region internal methods
        internal static bool Upload(MonthlyAllowance _ma)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("UploadMonthlyAllowance"))
                {
                    db.AddInParameter(dbCommand, "EmployeeCode", SqlDbType.VarChar, _ma.EmployeeCode);
                    db.AddInParameter(dbCommand, "HeadId", SqlDbType.SmallInt, _ma.HeadId);
                    db.AddInParameter(dbCommand, "Amount", SqlDbType.Float, _ma.Amount);
                    db.AddInParameter(dbCommand, "Remarks", SqlDbType.VarChar, _ma.Remarks);
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