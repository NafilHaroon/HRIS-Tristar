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
    public class JVBookList : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Order = 0, Name = "Voucher #")]
        public string VoucherNo { get; set; }
        [DataMember(Order = 1, Name = "Voucher Date")]
        public string VoucherDate { get; set; }
        public string Remarks { get; set; }
        #endregion

        #region constructor
        public JVBookList()
        {
        }

        public JVBookList(string _voucherNo, string _voucherDate, string _remarks)
        {
            VoucherNo = _voucherNo;
            VoucherDate = _voucherDate;
            Remarks = _remarks;
        }
        #endregion

        #region internal methods
        internal static List<JVBookList> Get()
        {
            List<JVBookList> lstJV = new List<JVBookList>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetPFJVouchers"))
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
                                lstJV.Add(new JVBookList(
                                    dr["VoucherNo"].ToString(),
                                    dr["VoucherDate"].ToString(),
                                    dr["Remarks"].ToString()
                                ));
                            }
                        }
                    }
                }
            }
            return lstJV;
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