using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.PF.Models
{
    public class BankBookList : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order = 0, Name = "Vouchere #")]
        public string VoucherNo { get; set; }
        [DataMember(Order = 1, Name = "Date")]
        public string VoucherDate { get; set; }
        [DataMember(Order = 2, Name = "Bank A/c")]
        public string BankAccountName { get; set; }
        [DataMember(Order = 3, Name = "Payee")]
        public string PayeeName { get; set; }
        [DataMember(Order = 4, Name = "Remarks")]
        public string Remarks { get; set; }
        #endregion

        #region constructor
        public BankBookList()
        {
        }

        public BankBookList(string _voucherNo, string _voucherDate, string _bankAccountName, 
            string _payeeName, string _remarks)
        {
            VoucherNo = _voucherNo;
            VoucherDate = _voucherDate;
            BankAccountName = _bankAccountName;
            PayeeName = _payeeName;
            Remarks = _remarks;
        }
        #endregion

        #region internal methods
        internal static List<BankBookList> Get()
        {
            List<BankBookList> lstBV = new List<BankBookList>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetPFBankVouchers"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                {
                    using (DataSet ds = db.ExecuteDataSet(dbCommand))
                    {
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                lstBV.Add(new BankBookList(
                                    dr["VoucherNo"].ToString(),
                                    dr["VoucherDate"].ToString(),
                                    dr["BankName"].ToString(),
                                    dr["PayeeName"].ToString(),
                                    dr["Remarks"].ToString()));
                            }
                        }
                    }
                }
            }
            return lstBV;
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