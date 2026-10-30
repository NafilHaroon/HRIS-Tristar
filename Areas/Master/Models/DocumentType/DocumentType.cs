using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class DocumentType : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; }
        public bool Mandatory { get; set; } = true;
        public bool Expires { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public Footer AuditTrail { get; set; } = new Footer();
        #endregion

        #region constructor
        public DocumentType()
        {

        }
        #endregion

        #region internal methods
        internal static DocumentType Get(short _typeId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetDocumentTypeById"))
            {
                db.AddInParameter(dbCommand, "DocumentTypeId", SqlDbType.TinyInt, _typeId);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (DocumentType _dt = new DocumentType())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _dt.DocumentTypeId = _typeId;
                            _dt.DocumentTypeName = dr["DocumentTypeName"].ToString();
                            _dt.Mandatory = Convert.ToBoolean(dr["Mandatory"]);
                            _dt.Expires = Convert.ToBoolean(dr["Expires"]);
                            _dt.IsActive = Convert.ToBoolean(dr["IsActive"]);
                            _dt.AuditTrail = new Footer(dr);
                            return _dt;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(DocumentType _dt)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveDocumentType"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "DocumentTypeId", SqlDbType.TinyInt, _dt.DocumentTypeId);
                    db.AddInParameter(dbCommand, "DocumentTypeName", SqlDbType.VarChar, _dt.DocumentTypeName);
                    db.AddInParameter(dbCommand, "Mandatory", SqlDbType.Float, _dt.Mandatory);
                    db.AddInParameter(dbCommand, "Expires", SqlDbType.Float, _dt.Expires);
                    db.AddInParameter(dbCommand, "IsActive", SqlDbType.Bit, _dt.IsActive);
                    db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _dt.AuditTrail.UpdatedOn);
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
