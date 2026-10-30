using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class TaxSetup : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? TaxYear { get; set; }
        public decimal PFThreshold { get; set; }
        public Footer AuditTrail { get; set; }
        public List<TaxSetupDetail> Details { get; set; }
        #endregion

        #region constructor
        public TaxSetup()
        {
            Details = new List<TaxSetupDetail>();
            AuditTrail = new Footer();
        }
        #endregion

        #region internal methods
        internal static TaxSetup Get(short _taxYear)
        {
            try
            {
                using (TaxSetup _ts = new TaxSetup())
                {
                    using (DbCommand dbCommand = db.GetStoredProcCommand("GetTaxSetupByYear"))
                    {
                        db.AddInParameter(dbCommand, "TaxYear", SqlDbType.SmallInt, _taxYear);
                        using (DataSet ds = db.ExecuteDataSet(dbCommand))
                        {
                            if (ds != null && ds.Tables.Count > 0)
                            {
                                DataRow dr = ds.Tables[0].Rows[0];
                                _ts.TaxYear = _taxYear;
                                _ts.PFThreshold = Convert.ToDecimal(dr["PFThreshold"]);
                                _ts.AuditTrail.CreatedBy = dr["CreatedBy"].ToString();
                                _ts.AuditTrail.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                                _ts.AuditTrail.UpdatedBy = dr["UpdatedBy"].ToString();
                                if (dr["UpdatedOn"] != DBNull.Value)
                                    _ts.AuditTrail.UpdatedOn = Convert.ToDateTime(dr["UpdatedOn"]);
                                _ts.Details = TaxSetupDetail.Get(_taxYear);
                                return _ts;
                            }
                            else
                                return null;
                        }
                    }
                }
            }
            catch (Exception)
            { throw; }
        }

        internal static bool Save(TaxSetup _ts)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveTaxSetup"))
            {
                db.AddInParameter(dbCommand, "TaxYear", SqlDbType.SmallInt, _ts.TaxYear);
                db.AddInParameter(dbCommand, "PFThreshold", SqlDbType.Decimal, _ts.PFThreshold);
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _ts.AuditTrail.UpdatedOn);
                using (DbConnection dbconnection = db.CreateConnection())
                {
                    dbconnection.Open();
                    DbTransaction transaction = dbconnection.BeginTransaction();
                    try
                    {
                        db.ExecuteNonQuery(dbCommand, transaction);
                        TaxSetupDetail.Save(_ts.TaxYear.Value, _ts.Details, transaction);
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

        #region IDisposable Members
        public void Dispose()
        {
            //
        }
        #endregion
    }
}