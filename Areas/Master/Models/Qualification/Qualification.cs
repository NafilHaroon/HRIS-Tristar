using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class Qualification : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? QualificationId { get; set; }
        public string QualificationName { get; set; }
        public bool IsActive { get; set; } = true;
        public Footer AuditTrail { get; set; } = new Footer();
        #endregion

        #region Constructor
        public Qualification()
        {
            //AuditTrail = new Footer();
            //IsActive = true;
        }
        #endregion

        #region internal methods
        internal static Qualification Get(short _qualificationId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetQualificationById"))
            {
                db.AddInParameter(dbCommand, "QualificationId", SqlDbType.TinyInt, _qualificationId);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (Qualification _qual = new Qualification())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _qual.QualificationId = _qualificationId;
                            _qual.QualificationName = dr["QualificationName"].ToString();
                            _qual.IsActive = Convert.ToBoolean(dr["IsActive"]);
                            _qual.AuditTrail = new Footer(dr);
                            return _qual;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(Qualification _qual)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveQualification"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "QualificationId", SqlDbType.SmallInt, _qual.QualificationId);
                    db.AddInParameter(dbCommand, "QualificationName", SqlDbType.VarChar, _qual.QualificationName);
                    db.AddInParameter(dbCommand, "IsActive", SqlDbType.Bit, _qual.IsActive);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _qual.AuditTrail.UpdatedOn);
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