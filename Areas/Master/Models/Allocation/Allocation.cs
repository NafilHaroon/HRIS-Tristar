using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class Allocation : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? TemplateId { get; set; }
        public string TemplateName { get; set; }
        public bool IsActive { get; set; } = true;
        public Footer AuditTrail { get; set; } = new Footer();
        public List<AllocationDetail> Details { get; set; } = new List<AllocationDetail>();
        #endregion

        #region constructor
        public Allocation()
        {
            //IsActive = true;
            //Details = new List<AllocationDetail>();
            //AuditTrail = new Footer();
        }
        #endregion

        #region internal methods
        internal static Allocation Get(short _templateId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetAllocationTemplateById"))
            {
                db.AddInParameter(dbCommand, "TemplateId", SqlDbType.SmallInt, _templateId);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (Allocation _at = new Allocation())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _at.TemplateId = _templateId;
                            _at.TemplateName = dr["TemplateName"].ToString();
                            _at.IsActive = Convert.ToBoolean(dr["IsActive"]);
                            _at.AuditTrail = new Footer(dr);
                            _at.Details = AllocationDetail.Get(_at.TemplateId.Value);                            
                            return _at;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(Allocation _at)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveAllocationTemplate"))
            {
                db.AddInParameter(dbCommand, "TemplateId", SqlDbType.SmallInt, _at.TemplateId);
                db.AddInParameter(dbCommand, "TemplateName", SqlDbType.VarChar, _at.TemplateName);
                db.AddInParameter(dbCommand, "IsActive", SqlDbType.Bit, _at.IsActive);
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                db.AddOutParameter(dbCommand, "newTemplateId", SqlDbType.SmallInt, 32);
                db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _at.AuditTrail.UpdatedOn);

                using (DbConnection dbconnection = db.CreateConnection())
                {
                    dbconnection.Open();
                    DbTransaction transaction = dbconnection.BeginTransaction();
                    try
                    {
                        db.ExecuteNonQuery(dbCommand, transaction);
                        _at.TemplateId = Convert.ToInt16(dbCommand.Parameters["@newTemplateId"].Value);
                        AllocationDetail.Save(_at.TemplateId.Value, _at.Details, transaction);
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