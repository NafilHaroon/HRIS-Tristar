using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.PF.Models
{
    public class JVBook : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? VoucherId { get; set; }
        public string VoucherNo { get; set; }
        public DateTime? VoucherDate { get; set; }
        public short? CompanyId { get; set; }
        public string Remarks { get; set; }
        public Footer AuditTrail { get; set; }
        public List<JVBookDetail> Details { get; set; }
        #endregion

        #region constructor
        public JVBook()
        {
            VoucherDate = DateTime.Now;          
            Details = new List<JVBookDetail>();
            CompanyId = ServerSession.GetCurrentCompany();
            AuditTrail = new Footer();
        }
        #endregion

        #region internal methods
        internal static JVBook Get(string _voucherNo)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetPFJVoucherByNo"))
            {
                db.AddInParameter(dbCommand, "VoucherNo", SqlDbType.VarChar, _voucherNo);
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (JVBook _jb = new JVBook())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _jb.VoucherNo = _voucherNo;
                            _jb.VoucherId = Convert.ToInt16(dr["VoucherId"]);
                            _jb.VoucherDate = Convert.ToDateTime(dr["VoucherDate"]);
                            _jb.CompanyId = Convert.ToInt16(dr["CompanyId"]);
                            _jb.Remarks = dr["Remarks"].ToString();
                            _jb.AuditTrail.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                            _jb.AuditTrail.CreatedBy = dr["CreatedBy"].ToString();
                            _jb.Details = JVBookDetail.Get(_jb.VoucherId.Value);
                            return _jb;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(JVBook _jv)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SavePFJVoucher"))
            {
                db.AddInParameter(dbCommand, "VoucherDate", SqlDbType.DateTime, _jv.VoucherDate);
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, _jv.CompanyId);
                db.AddInParameter(dbCommand, "Remarks", SqlDbType.VarChar, _jv.Remarks);
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
                        _jv.VoucherId = Convert.ToInt16(dbCommand.Parameters["@VoucherId"].Value);
                        _jv.VoucherNo = dbCommand.Parameters["@VoucherNo"].Value.ToString();
                        JVBookDetail.Save(_jv.VoucherId.Value, _jv.Details, transaction);
                        Post(_jv.VoucherId.Value, transaction);
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
                using (DbCommand dbCommand = db.GetStoredProcCommand("PostPFJVoucher"))
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