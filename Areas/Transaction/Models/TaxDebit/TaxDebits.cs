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
    public class TaxDebits : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order = 0, Name = "Ref #")]
        public short TaxDebitId { get; set; }
        [DataMember(Order = 1, Name = "Employee Name")]
        public string EmployeeName { get; set; }
        [DataMember(Order = 2, Name = "Tax Debit Type")]
        public string TaxDebitTypeName { get; set; }
        [DataMember(Order =3, Name = "Amount")]
        public decimal Amount { get; set; }
        #endregion

        #region constructor
        public TaxDebits()
        {
        }

        //public TaxDebits(short _debitId, string _employeeName, string _debitTypeName, decimal _amount)
        //{
        //    TaxDebitId = _debitId;
        //    EmployeeName = _employeeName;
        //    TaxDebitTypeName = _debitTypeName;
        //    Amount = _amount;
        //}
        #endregion

        #region internal methods
        internal static List<TaxDebits> Get()
        {
            List<TaxDebits> debits = new List<TaxDebits>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetTaxDebits"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            debits.Add(new TaxDebits
                            {
                                TaxDebitId = Convert.ToInt16(dr["TaxDebitId"]),
                                EmployeeName = dr["EmployeeName"].ToString(),
                                TaxDebitTypeName = dr["TaxDebitTypeName"].ToString(),
                                Amount = Convert.ToDecimal(dr["Amount"])
                            });
                        }
                    }
                }
            }
            return debits;
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