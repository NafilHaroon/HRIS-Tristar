using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class TaxCreditType : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? TaxCreditTypeId { get; set; }
        public string TaxCreditTypeName { get; set; }
        public short? CreditRuleId { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal MaxRate { get; set; }
        public bool IsActive { get; set; }
        public Footer AuditTrail { get; set; }
        #endregion

        #region constructor
        public TaxCreditType()
        {
            AuditTrail = new Footer();
            IsActive = true;
        }
        #endregion

        #region internal methods
        internal static TaxCreditType Get(short _typeId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetTaxCreditTypeById"))
            {
                db.AddInParameter(dbCommand, "TaxCreditTypeId", SqlDbType.TinyInt, _typeId);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (TaxCreditType _ct = new TaxCreditType())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _ct.TaxCreditTypeId = _typeId;
                            _ct.TaxCreditTypeName = dr["TaxCreditTypeName"].ToString();
                            _ct.CreditRuleId = Convert.ToInt16(dr["CreditRuleId"]);
                            _ct.MaxAmount = Convert.ToDecimal(dr["MaxAmount"]);
                            _ct.MaxRate = Convert.ToDecimal(dr["MaxRate"]);
                            _ct.IsActive = Convert.ToBoolean(dr["IsActive"]);
                            _ct.AuditTrail.CreatedBy = dr["CreatedBy"].ToString();
                            _ct.AuditTrail.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                            _ct.AuditTrail.UpdatedBy = dr["UpdatedBy"].ToString();
                            if (dr["UpdatedOn"] != DBNull.Value)
                                _ct.AuditTrail.UpdatedOn = Convert.ToDateTime(dr["UpdatedOn"]);
                            return _ct;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(TaxCreditType _ct)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveTaxCreditType"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "TaxCreditTypeId", SqlDbType.TinyInt, _ct.TaxCreditTypeId);
                    db.AddInParameter(dbCommand, "TaxCreditTypeName", SqlDbType.VarChar, _ct.TaxCreditTypeName);
                    db.AddInParameter(dbCommand, "CreditRuleId", SqlDbType.TinyInt, _ct.CreditRuleId);
                    db.AddInParameter(dbCommand, "MaxAmount", SqlDbType.Decimal, _ct.MaxAmount);
                    db.AddInParameter(dbCommand, "MaxRate", SqlDbType.Decimal, _ct.MaxRate);
                    db.AddInParameter(dbCommand, "IsActive", SqlDbType.Bit, _ct.IsActive);
                    db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _ct.AuditTrail.UpdatedOn);
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
