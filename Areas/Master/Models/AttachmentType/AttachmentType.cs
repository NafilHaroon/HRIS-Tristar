using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class AttachmentType : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? AttachmentTypeId { get; set; }
        public string AttachmentTypeName { get; set; }
        public bool IsActive { get; set; } = true;
        public Footer AuditTrail { get; set; } = new Footer();
        #endregion

        #region constructor
        public AttachmentType()
        {
            AuditTrail = new Footer();
            IsActive = true;
        }
        #endregion

        #region internal methods
        internal static AttachmentType Get(short _typeId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetAttachmentTypeById"))
            {
                db.AddInParameter(dbCommand, "AttachmentTypeId", SqlDbType.TinyInt, _typeId);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (AttachmentType _at = new AttachmentType())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _at.AttachmentTypeId = _typeId;
                            _at.AttachmentTypeName = dr["AttachmentTypeName"].ToString();
                            _at.IsActive = Convert.ToBoolean(dr["IsActive"]);
                            _at.AuditTrail = new Footer(dr);
                            return _at;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(AttachmentType _at)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveAttachmentType"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "AttachmentTypeId", SqlDbType.TinyInt, _at.AttachmentTypeId);
                    db.AddInParameter(dbCommand, "AttachmentTypeName", SqlDbType.VarChar, _at.AttachmentTypeName);
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
