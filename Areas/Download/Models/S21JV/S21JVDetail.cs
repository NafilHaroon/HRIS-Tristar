using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Download.Models
{
    public class S21JVDetail : IDisposable
    {
        #region private properties
        private static readonly SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string AccountCode { get; set; }
        public string Narration { get; set; }
        public double Salary { get; set; }
        public int Qty { get; set; }
        public string Period { get; set; }
        #endregion

        #region constructor
        public S21JVDetail()
        {
        }

        public S21JVDetail(string _accountCode, string _narration, 
            double _salary, int _qty, string _period)
        {
            AccountCode = _accountCode;
            Narration = _narration;
            Salary = _salary;
            Qty = _qty;
            Period = _period;
        }
        #endregion

        #region internal methods
        internal static List<S21JVDetail> Get(short _periodId)
        {
            List<S21JVDetail> lstS21 = new List<S21JVDetail>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("extPostedSalaryFinance"))
            {
                db.AddInParameter(dbCommand, "PeriodId", SqlDbType.SmallInt, _periodId);
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            lstS21.Add(new S21JVDetail(
                                dr["AccountCode"].ToString(),
                                dr["Narration"].ToString(),
                                Convert.ToDouble(dr["Amount"]),
                                0,
                                dr["PeriodName"].ToString()));
                        }
                    }
                }
            }
            return lstS21;
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