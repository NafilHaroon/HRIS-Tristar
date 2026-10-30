using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Transaction.Models
{
    public class TaxCredits : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order = 0, Name = "Ref #")]
        public short TaxCreditId { get; set; }
        [DataMember(Order = 1, Name = "Employee Name")]
        public string EmployeeName { get; set; }
        [DataMember(Order = 2, Name = "Tax Credit Type")]
        public string TaxCreditTypeName { get; set; }
        [DataMember(Order =3, Name = "Amount")]
        public decimal Amount { get; set; }
        public string StateName { get; set; }
        #endregion

        #region constructor
        public TaxCredits()
        {
        }

        //public TaxCredits(short _creditId, string _employeeName, string _creditTypeName, decimal _amount, string _status)
        //{
        //    TaxCreditId = _creditId;
        //    EmployeeName = _employeeName;
        //    TaxCreditTypeName = _creditTypeName;
        //    Amount = _amount;
        //    Status = _status;
        //}
        #endregion

        #region internal methods
        internal static List<TaxCredits> Get()
        {
            List<TaxCredits> credits = new List<TaxCredits>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetTaxCredits"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            credits.Add(new TaxCredits
                            {
                                TaxCreditId = Convert.ToInt16(dr["TaxCreditId"]),
                                EmployeeName = dr["EmployeeName"].ToString(),
                                TaxCreditTypeName = dr["TaxCreditTypeName"].ToString(),
                                Amount = Convert.ToDecimal(dr["Amount"]),
                                StateName = dr["StateName"].ToString()
                            });
                        }
                    }
                }
            }
            return credits;
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