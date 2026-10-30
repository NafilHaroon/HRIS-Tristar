using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.PF.Models
{
    public class CoA : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public int? AccountId { get; set; }
        public string AccountCode { get; set; }
        public short? CompanyId { get; set; }
        public string AccountName { get; set; }
        public short? AccountTypeId { get; set; }
        public double? OpeningBalance { get; set; }
        public bool BankAccount { get; set; }
        public Footer AuditTrail { get; set; }
        #endregion

        #region constructor
        public CoA()
        {
            AuditTrail = new Footer();
        }
        #endregion

        #region internal methods
        internal static CoA Get(string _accountCode)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetCoAByCode"))
            {
                db.AddInParameter(dbCommand, "AccountCode", SqlDbType.VarChar, _accountCode);
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (CoA _coa = new CoA())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _coa.AccountId = Convert.ToInt32(dr["AccountId"]);
                            _coa.AccountCode = dr["AccountCode"].ToString();
                            _coa.CompanyId =  Convert.ToInt16(dr["CompanyId"]);
                            _coa.AccountName = dr["AccountName"].ToString();
                            _coa.AccountTypeId = Convert.ToInt16(dr["AccountTypeId"]);
                            _coa.OpeningBalance = Convert.ToDouble(dr["OpeningBalance"]);
                            _coa.BankAccount = Convert.ToBoolean(dr["BankAccount"]);
                            _coa.AuditTrail.CreatedBy = dr["CreatedBy"].ToString();
                            _coa.AuditTrail.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                            _coa.AuditTrail.UpdatedBy = dr["UpdatedBy"].ToString();
                            if (dr["UpdatedOn"] != DBNull.Value)
                                _coa.AuditTrail.UpdatedOn = Convert.ToDateTime(dr["UpdatedOn"]);
                         
                            return _coa;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(CoA _coa)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveCoA"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "AccountId", SqlDbType.SmallInt, _coa.AccountId);
                    db.AddInParameter(dbCommand, "CompanyId", SqlDbType.TinyInt, ServerSession.GetCurrentCompany());
                    db.AddInParameter(dbCommand, "AccountName", SqlDbType.VarChar, _coa.AccountName);
                    db.AddInParameter(dbCommand, "AccountTypeId", SqlDbType.TinyInt, _coa.AccountTypeId);
                    db.AddInParameter(dbCommand, "OpeningBalance", SqlDbType.Float, _coa.OpeningBalance);
                    db.AddInParameter(dbCommand, "BankAccount", SqlDbType.Bit, _coa.BankAccount);
                    db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _coa.AuditTrail.UpdatedOn);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
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
