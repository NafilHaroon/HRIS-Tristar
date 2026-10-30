using HRIS.Areas.Common.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace HRIS.Areas.Master.Models
{
    public class Employees : IDisposable
    {
        #region private properties
        private static SqlDatabase db = DatabaseFactory.CreateDatabase() as SqlDatabase;
        #endregion

        #region public properties
        [DataMember(Name = "Employee Code", Order = 0)]
        public string EmployeeCode { get; set; }
        [DataMember(Name = "Employee Id", Order = 1)]
        public short EmployeeId { get; set; }
        [DataMember(Name = "Employee Name", Order = 2)]
        public string EmployeeName { get; set; }
        [DataMember(Name = "Branch Name", Order = 3)]
        public string BranchName { get; set; }
        [DataMember(Name = "Department Name", Order = 4)]
        public string DepartmentName { get; set; }
        [DataMember(Name = "Status", Order = 5)]
        public string StatusName { get; set; }
        #endregion

        #region constructor
        public Employees()
        {
        }

        //public Employees(string _employeeCode, string _employeeName)
        //{
        //    EmployeeCode = _employeeCode;
        //    EmployeeName = _employeeName;
        //}

        //public Employees(short _employeeId, string _employeeName)
        //{
        //    EmployeeId = _employeeId;
        //    EmployeeName = _employeeName;
        //}

        //public Employees(string _employeeCode, short _employeeId, string _employeeName)
        //{
        //    EmployeeCode = _employeeCode;
        //    EmployeeId = _employeeId;
        //    EmployeeName = _employeeName;
        //}

        //public Employees(string _employeeCode, short _employeeId, string _employeeName, string _branchName, string _departmentName, string _statusName)
        //{
        //    EmployeeCode = _employeeCode;
        //    EmployeeId = _employeeId;
        //    EmployeeName = _employeeName;
        //    BranchName = _branchName;
        //    DepartmentName = _departmentName;
        //    StatusName = _statusName;
        //}
        #endregion

        #region internal methods
        internal static List<Employees> Get()
        {
            List<Employees> lstEmp = new List<Employees>();
            using (DbCommand dbCommand = db.GetStoredProcCommand("GetEmployees"))
            {
                db.AddInParameter(dbCommand, "CompanyId", SqlDbType.SmallInt, ServerSession.GetCurrentCompany());
                db.AddInParameter(dbCommand, "UserId", SqlDbType.VarChar, ServerSession.GetCurrentUser());
                using (DataTable dt = db.ExecuteDataSet(dbCommand).Tables[0])
                {
                    if (dt != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            lstEmp.Add(new Employees
                            {
                                EmployeeCode = dr["EmployeeCode"].ToString(),
                                EmployeeId = Convert.ToInt16(dr["EmployeeId"]),
                                EmployeeName = dr["EmployeeName"].ToString(),
                                BranchName = dr["BranchName"].ToString(),
                                DepartmentName = dr["DepartmentName"].ToString(),
                                StatusName = dr["StatusName"].ToString()
                            });
                        }
                    }
                }
            }
            return lstEmp;
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