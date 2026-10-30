using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class Department : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public short? HFMSubCategoryId { get; set; }
        public bool IsActive { get; set; } = true;
        public Footer AuditTrail { get; set; } = new Footer();
        #endregion

        #region constructor
        public Department()
        {
            //AuditTrail = new Footer();
            //IsActive = true;
        }
        #endregion

        #region internal methods
        internal static Department Get(string _departmentCode)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetDepartmentByCode"))
            {
                db.AddInParameter(dbCommand, "DepartmentCode", SqlDbType.VarChar, _departmentCode);
                using (DataSet ds = db.ExecuteDataSet(dbCommand))
                {
                    if (ds.Tables != null && ds.Tables[0].Rows.Count > 0)
                    {
                        using (Department _d = new Department())
                        {
                            DataRow dr = ds.Tables[0].Rows[0];
                            _d.DepartmentCode = dr["DepartmentCode"].ToString();
                            _d.DepartmentName = dr["DepartmentName"].ToString();
                            if (dr["HFMSubCategoryId"] != DBNull.Value)
                                _d.HFMSubCategoryId = Convert.ToInt16(dr["HFMSubCategoryId"]);
                            _d.IsActive = Convert.ToBoolean(dr["IsActive"]);
                            _d.AuditTrail = new Footer(dr);
                            return _d;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        internal static bool Save(Department _d)
        {
            using (DbCommand dbCommand = db.GetStoredProcCommand("SaveDepartment"))
            {
                try
                {
                    db.AddInParameter(dbCommand, "DepartmentCode", SqlDbType.VarChar, _d.DepartmentCode);
                    db.AddInParameter(dbCommand, "HFMSubCategoryId", SqlDbType.SmallInt, _d.HFMSubCategoryId);
                    db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                    db.AddInParameter(dbCommand, "UpdatedOn", SqlDbType.DateTime, _d.AuditTrail.UpdatedOn);
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
