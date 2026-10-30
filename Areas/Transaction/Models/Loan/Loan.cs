using HRIS.Areas.Common.Models;
using HRIS.Areas.Master.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Transaction.Models
{
    public class Loan : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? LoanId { get; set; }
        //public short? EmployeeId { get; set; }
        public EmployeeShortList Employee { get; set; }
        public short? PeriodId { get; set; }
        public short? LoanTypeId { get; set; }
        public double LoanAmount { get; set; }
        public double Balance { get; set; }
        public short? Installments { get; set; }
        public string Reason { get; set; }
        public string StatusName { get; set; }
        public Footer AuditTrail { get; set; }
        //public DateTime? EffectiveDate { get; set; }
        #endregion

        #region constructor
        public Loan()
        {
            StatusName = "Open";
            AuditTrail = new Footer();
            Employee = new EmployeeShortList();
            //IsActive = true;
            //Settled = false;
            PeriodId = ServerSession.GetCurrentPeriod();
        }
        #endregion

        #region internal methods
        internal static Loan Get(short _loanId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetLoanById"))
            {
                db.AddInParameter(dbCommand, "LoanId", SqlDbType.SmallInt, _loanId);
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (Loan _l = new Loan())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _l.LoanId = _loanId;
                            //_l.EmployeeId = Convert.ToInt16(dr["EmployeeId"]);
                            _l.Employee = new EmployeeShortList { EmployeeId = Convert.ToInt16(dr["EmployeeId"]), EmployeeName = dr["EmployeeName"].ToString() };
                            _l.PeriodId = Convert.ToInt16(dr["StartPeriodId"]);
                            _l.LoanTypeId = Convert.ToInt16(dr["LoanTypeId"]);
                            _l.LoanAmount = Convert.ToDouble(dr["LoanAmount"]);
                            _l.Balance = Convert.ToDouble(dr["Balance"]);
                            _l.Installments = Convert.ToInt16(dr["Installments"]);
                            _l.StatusName = dr["StatusName"].ToString();
                            //_l.IsActive = Convert.ToBoolean(dr["IsActive"]);
                            //_l.Settled = Convert.ToBoolean(dr["Settled"]);
                            _l.Reason = dr["Reason"].ToString();
                            _l.AuditTrail.CreatedBy = dr["CreatedBy"].ToString();
                            _l.AuditTrail.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                            _l.AuditTrail.UpdatedBy = dr["UpdatedBy"].ToString();
                            if (dr["UpdatedOn"] != DBNull.Value)
                                _l.AuditTrail.UpdatedOn = Convert.ToDateTime(dr["UpdatedOn"]);
                            return _l;
                            //                            _l.EffectiveDate = Convert.ToDateTime(dr["StartingDate"]);
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(Loan _ls)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveLoan"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "LoanId", SqlDbType.SmallInt, _ls.LoanId);
                    db.AddInParameter(dbCommand, "EmployeeId", SqlDbType.SmallInt, _ls.Employee.EmployeeId);//_ls.EmployeeId);
                    db.AddInParameter(dbCommand, "PeriodId", SqlDbType.SmallInt, _ls.PeriodId);
                    db.AddInParameter(dbCommand, "LoanTypeId", SqlDbType.SmallInt, _ls.LoanTypeId);
                    db.AddInParameter(dbCommand, "LoanAmount", SqlDbType.Float, _ls.LoanAmount);
                    db.AddInParameter(dbCommand, "Installments", SqlDbType.SmallInt, _ls.Installments);
                    db.AddInParameter(dbCommand, "Reason", SqlDbType.VarChar, _ls.Reason);
                    //db.AddInParameter(dbCommand, "IsActive", SqlDbType.Bit, _ls.IsActive);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _ls.AuditTrail.UpdatedOn);
                    //db.AddInParameter(dbCommand, "EffectiveDate", SqlDbType.DateTime, _ls.EffectiveDate);
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