using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.PF.Models
{
    public class BankBook : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? VoucherId { get; set; }
        public string VoucherNo { get; set; }
        public DateTime? VoucherDate { get; set; }
        public short? CompanyId { get; set; }
        public short? BankAccountId { get; set; }
        public string ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string PayeeName { get; set; }
        public string Remarks { get; set; }
        public Footer AuditTrail { get; set; }
        public List<BankBookDetail> Details { get; set; }
        #endregion

        #region constructor
        public BankBook()
        {
            VoucherDate = DateTime.Now;
            ChequeDate = DateTime.Now;
            Details = new List<BankBookDetail>();
            CompanyId = ServerSession.GetCurrentCompany();
            AuditTrail = new Footer();
        }
        #endregion

        #region internal methods
        internal static BankBook Get(string _voucherNo)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetPFBankVoucherByNo"))
            {
                db.AddInParameter(dbCommand, "VoucherNo", SqlDbType.VarChar, _voucherNo);
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (BankBook _bb = new BankBook())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _bb.VoucherNo = _voucherNo;
                            _bb.VoucherId = Convert.ToInt16(dr["VoucherId"]);
                            _bb.VoucherDate = Convert.ToDateTime(dr["VoucherDate"]);
                            _bb.CompanyId = Convert.ToInt16(dr["CompanyId"]);
                            _bb.BankAccountId = Convert.ToInt16(dr["BankAccountId"]);
                            _bb.ChequeNo = dr["ChequeNo"].ToString();
                            if (dr["ChequeDate"] != DBNull.Value)
                                _bb.ChequeDate = Convert.ToDateTime(dr["ChequeDate"]);
                            _bb.PayeeName = dr["PayeeName"].ToString();
                            _bb.Remarks = dr["Remarks"].ToString();
                            _bb.AuditTrail.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                            _bb.AuditTrail.CreatedBy = dr["CreatedBy"].ToString();

                            _bb.Details = BankBookDetail.Get(_bb.VoucherId.Value);
                            return _bb;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(BankBook _bb)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SavePFBankVoucher"))
            {
                db.AddInParameter(dbCommand, "VoucherDate", SqlDbType.DateTime, _bb.VoucherDate);
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "BankAccountId", SqlDbType.SmallInt, _bb.BankAccountId);
                db.AddInParameter(dbCommand, "ChequeNo", SqlDbType.VarChar, _bb.ChequeNo);
                db.AddInParameter(dbCommand, "ChequeDate", SqlDbType.DateTime, _bb.ChequeDate);
                db.AddInParameter(dbCommand, "PayeeName", SqlDbType.VarChar, _bb.PayeeName);
                db.AddInParameter(dbCommand, "Remarks", SqlDbType.VarChar, _bb.Remarks);
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());

                db.AddOutParameter(dbCommand, "VoucherId", SqlDbType.SmallInt, 32);
                db.AddOutParameter(dbCommand, "VoucherNo", SqlDbType.VarChar, 10);

                using (DbConnection dbconnection = db.CreateConnection())
                {
                    dbconnection.Open();
                    DbTransaction transaction = dbconnection.BeginTransaction();
                    try
                    {
                        db.ExecuteNonQuery(dbCommand, transaction);
                        _bb.VoucherId = Convert.ToInt16(dbCommand.Parameters["@VoucherId"].Value);
                        _bb.VoucherNo = dbCommand.Parameters["@VoucherNo"].Value.ToString();
                        BankBookDetail.Save(_bb.VoucherId.Value, _bb.Details, transaction);
                        Post(_bb.VoucherId.Value, transaction);
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
        }
        #endregion

        #region private methods
        private static bool Post(int _voucherId, DbTransaction _transaction)
        {
            try
            {
                using (DbCommand dbCommand = db.GetStoredProcCommand("PostPFBankVoucher"))
                {
                    db.AddInParameter(dbCommand, "VoucherId", SqlDbType.Int, _voucherId);
                    db.ExecuteNonQuery(dbCommand, _transaction);
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