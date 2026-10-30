using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class Grade : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public short? GradeId { get; set; }
        public string GradeNo { get; set; }
        public string GradeName { get; set; }
        public bool IsActive { get; set; } = true;
        public Footer AuditTrail { get; set; } = new Footer();
        #endregion

        #region constructor
        public Grade()
        {
        }
        #endregion

        #region internal methods
        internal static Grade Get(short _gradeId)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetGradeById"))
            {
                db.AddInParameter(dbCommand, "GradeId", SqlDbType.SmallInt, _gradeId);
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (Grade _grd = new Grade())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _grd.GradeId = Convert.ToInt16(dr["GradeId"]);
                            _grd.GradeNo = dr["GradeNo"].ToString();
                            _grd.GradeName = dr["GradeName"].ToString();
                            _grd.IsActive = Convert.ToBoolean(dr["IsActive"]);
                            _grd.AuditTrail = new Footer(dr);
                            return _grd;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(Grade _grd)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveGrade"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "GradeId", SqlDbType.TinyInt, _grd.GradeId);
                    db.AddInParameter(dbCommand, "GradeNo", SqlDbType.VarChar, _grd.GradeNo);
                    db.AddInParameter(dbCommand, "GradeName", SqlDbType.VarChar, _grd.GradeName);
                    db.AddInParameter(dbCommand, "IsActive", SqlDbType.Bit, _grd.IsActive);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _grd.AuditTrail.UpdatedOn);
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
