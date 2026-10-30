using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace HRIS.Areas.Master.Models
{
    public class Departments : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        #endregion

        #region constructor
        public Departments()
        {
        }
        //
        //public Departments(string _departmentCode, string _departmentName)
        //{
        //    DepartmentCode = _departmentCode;
        //    DepartmentName = _departmentName;
        //}
        #endregion

        #region internal methods
        internal static List<Departments> Get(bool _activeOnly = true)
        {
            List<Departments> departments = new List<Departments>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetDepartments"))
            {
                db.AddInParameter(dbCommand, "ActiveOnly", SqlDbType.Bit, _activeOnly);
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            departments.Add(new Departments
                            {
                                DepartmentCode = dr["DepartmentCode"].ToString(),
                                DepartmentName = dr["DepartmentName"].ToString()
                            });
                        }
                    }
                }
            }
            return departments;
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