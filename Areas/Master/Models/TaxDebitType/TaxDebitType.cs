using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class TaxDebitType : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? TaxDebitTypeId { get; set; }
        public string TaxDebitTypeName { get; set; }
        public bool IsActive { get; set; }
        public Footer AuditTrail { get; set; }
        #endregion

        #region constructor
        public TaxDebitType()
        {
            AuditTrail = new Footer();
            IsActive = true;
        }
        #endregion

        #region internal methods
        internal static TaxDebitType Get(short _typeId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetTaxDebitTypeById"))
            {
                db.AddInParameter(dbCommand, "TaxDebitTypeId", SqlDbType.TinyInt, _typeId);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (TaxDebitType _at = new TaxDebitType())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _at.TaxDebitTypeId = _typeId;
                            _at.TaxDebitTypeName = dr["TaxDebitTypeName"].ToString();
                            _at.IsActive = Convert.ToBoolean(dr["IsActive"]);
                            _at.AuditTrail.CreatedBy = dr["CreatedBy"].ToString();
                            _at.AuditTrail.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                            _at.AuditTrail.UpdatedBy = dr["UpdatedBy"].ToString();
                            if (dr["UpdatedOn"] != DBNull.Value)
                                _at.AuditTrail.UpdatedOn = Convert.ToDateTime(dr["UpdatedOn"]);
                            return _at;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(TaxDebitType _at)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveTaxDebitType"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "TaxDebitTypeId", SqlDbType.TinyInt, _at.TaxDebitTypeId);
                    db.AddInParameter(dbCommand, "TaxDebitTypeName", SqlDbType.VarChar, _at.TaxDebitTypeName);
                    db.AddInParameter(dbCommand, "IsActive", SqlDbType.Bit, _at.IsActive);
                    db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _at.AuditTrail.UpdatedOn);
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
