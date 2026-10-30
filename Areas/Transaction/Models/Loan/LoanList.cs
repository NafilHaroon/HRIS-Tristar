using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Transaction.Models
{
    public class LoanList : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order = 0, Name = "Loan Id")]
        public short LoanId { get; set; }
        [DataMember(Order = 1, Name = "Employee Name")]
        public string EmployeeName { get; set; }
        [DataMember(Order = 2, Name = "Loan Type")]
        public string LoanTypeName { get; set; }
        [DataMember(Order = 3, Name = "Starting Period")]
        public string PeriodName { get; set; }
        #endregion

        #region constructor
        public LoanList()
        {
        }

        public LoanList(short _loanId, string _employeeName, string _loanTypeName, string _periodName)
        {
            LoanId = _loanId;
            EmployeeName = _employeeName;
            LoanTypeName = _loanTypeName;
            PeriodName = _periodName;
        }
        #endregion

        #region internal methods
        internal static List<LoanList> Get()
        {
            List<LoanList> lstLoan = new List<LoanList>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetLoans"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstLoan.Add(new LoanList(
                                Convert.ToInt16(dr["LoanId"]),
                                dr["EmployeeName"].ToString(),
                                dr["LoanTypeName"].ToString(),
                                dr["PeriodName"].ToString()));
                        }
                    }
                }
            }
            return lstLoan;
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