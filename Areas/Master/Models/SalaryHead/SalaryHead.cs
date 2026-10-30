using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class SalaryHead : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? HeadId { get; set; }
        public string HeadName { get; set; }
        public short? HeadTypeId { get; set; }
        public double DefaultAmount { get; set; }
        public bool SalarySheet { get; set; }
        public bool DeductForAbsent { get; set; }
        public bool ITaxExempted { get; set; }
        public bool Arrears { get; set; }
        public bool IsActive { get; set; } = true;
        public bool SystemCalculated { get; set; } = false;
        public Footer AuditTrail { get; set; } = new Footer();
        public List<SalaryHeadDetail> Details { get; set; } = new List<SalaryHeadDetail>();
        #endregion

        #region constructor
        public SalaryHead()
        {
            //IsActive = true;
            //SystemCalculated = false;
            //AuditTrail = new Footer();
            //Details = new List<SalaryHeadDetail>();
        }
        #endregion

        #region internal methods
        internal static SalaryHead Get(short _headId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetSalaryHeadById"))
            {
                db.AddInParameter(dbCommand, "HeadId", SqlDbType.SmallInt, _headId);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (SalaryHead _sh = new SalaryHead())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _sh.HeadId = Convert.ToInt16(dr["HeadId"]);
                            _sh.HeadName = dr["HeadName"].ToString();
                            _sh.HeadTypeId = Convert.ToInt16(dr["HeadTypeId"]);
                            _sh.DefaultAmount = Convert.ToDouble(dr["DefaultAmount"]);
                            _sh.SalarySheet = Convert.ToBoolean(dr["SalarySheet"]);
                            _sh.DeductForAbsent = Convert.ToBoolean(dr["DeductForAbsent"]);
                            _sh.ITaxExempted = Convert.ToBoolean(dr["ITaxExempted"]);
                            _sh.Arrears = Convert.ToBoolean(dr["Arrears"]);
                            _sh.IsActive = Convert.ToBoolean(dr["IsActive"]);
                            _sh.SystemCalculated = Convert.ToBoolean(dr["SystemCalculated"]);
                            _sh.AuditTrail = new Footer(dr);
                            _sh.Details = SalaryHeadDetail.Get(_sh.HeadId.Value);
                            return _sh;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(SalaryHead _sh)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveSalaryHead"))
            {
                db.AddInParameter(dbCommand, "HeadId", SqlDbType.TinyInt, _sh.HeadId);
                db.AddInParameter(dbCommand, "HeadName", SqlDbType.VarChar, _sh.HeadName);
                db.AddInParameter(dbCommand, "HeadTypeId", SqlDbType.TinyInt, _sh.HeadTypeId);
                db.AddInParameter(dbCommand, "DefaultAmount", SqlDbType.Float, _sh.DefaultAmount);
                db.AddInParameter(dbCommand, "SalarySheet", SqlDbType.Bit, _sh.SalarySheet);
                db.AddInParameter(dbCommand, "DeductForAbsent", SqlDbType.Bit, _sh.DeductForAbsent);
                db.AddInParameter(dbCommand, "Arrears", SqlDbType.Bit, _sh.Arrears);
                db.AddInParameter(dbCommand, "ITaxExempted", SqlDbType.Bit, (_sh.HeadTypeId > 2 ? true : _sh.ITaxExempted));
                db.AddInParameter(dbCommand, "IsActive", SqlDbType.Bit, _sh.IsActive);
                db.AddInParameter(dbCommand, "SystemCalculated", SqlDbType.Bit, _sh.SystemCalculated);
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _sh.AuditTrail.UpdatedOn);
                db.AddOutParameter(dbCommand, "newHeadId", SqlDbType.SmallInt, 32);
                using (DbConnection dbconnection = db.CreateConnection())
                {
                    dbconnection.Open();
                    DbTransaction transaction = dbconnection.BeginTransaction();
                    try
                    {
                        db.ExecuteNonQuery(dbCommand, transaction);
                        _sh.HeadId = Convert.ToInt16(dbCommand.Parameters["@newHeadId"].Value);
                        SalaryHeadDetail.Save(_sh.HeadId.Value, _sh.Details, transaction);
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
            // no implementation
        }
        #endregion
    }
}
